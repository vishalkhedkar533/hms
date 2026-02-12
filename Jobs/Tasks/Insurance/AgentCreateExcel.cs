using CommonLibrary;
using CommonLibrary.mapping;
using Dapper;
using Database;
using MiniExcelLibs;
using Quartz;
using Repository;
using SharedModels.BackEndCalculation;
using SharedModels.DTO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class AgentCreateExcel
    {
        private readonly IMappingProvider _mappingProvider;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly string _baseDir = AppContext.BaseDirectory;
        private readonly ILogger<AgentCreateExcel> _logger;
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly IConnectionScope _connectionScope;
        private readonly IBinaryImportFactory _bulkOpsFactory;

        public AgentCreateExcel(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<AgentCreateExcel> logger,
            IConnectionScope connectionScope,
            IBinaryImportFactory bulkOpsFactory)
        {
            _jobExecutionContext = jobExecutionContext;
            orgId = int.Parse(jobExecutionContext.JobDetail.JobDataMap.Values
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.index.Equals(
                    jobExecutionContext.JobDetail.JobDataMap.Keys
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(x => x.value == "orgId").index))
                .value.ToString());
            jobKey = jobExecutionContext.JobDetail.Key;
            _mappingProvider = mappingProvider;
            _configuration = configuration;
            _logger = logger;
            _connectionScope = connectionScope ?? throw new ArgumentNullException(nameof(connectionScope));
            _bulkOpsFactory = bulkOpsFactory ?? throw new ArgumentNullException(nameof(bulkOpsFactory));
        }

        public async Task ProcessAgentCreateData(JobExeHist jobExeHist)
        {
            _logger.LogInformation("AgentCreateExcel job started for OrgId={OrgId}", orgId);

            // 1. Resolve Connection and Operation Scripts
            var operationMapping = _mappingProvider.GetScriptForOperation("Job", "Fetch")
                ?? throw new InvalidOperationException("Operation mapping for Job/Fetch not found.");

            var connectionString = _configuration.GetConnectionString(operationMapping.ConnectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{operationMapping.ConnectionStringKey}' not found.");

            // Use the scoped connection
            var conn = await _connectionScope.GetOpenConnectionAsync(connectionString);
            var token = CancellationToken.None;
            var validatorConfig = LoadValidatorConfig();

            // 2. Fetch Pending Tasks (Inlined)
            var tasksSql = _mappingProvider.GetScriptForOperation("Agent", "GetPendingTasks")?.Script
                ?? throw new Exception("SQL for GetPendingTasks missing");
            var tasks = await conn.QueryAsync<FileProcessingTask>(tasksSql);

            if (!tasks.Any())
            {
                _logger.LogInformation("No pending agent create tasks found");
                return;
            }

            // 3. Get Chunk Size (Inlined)
            var chunkSql = _mappingProvider.GetScriptForOperation("Agent", "GetChunkSize")?.Script
                ?? throw new Exception("SQL for GetChunkSize missing");
            int chunkSize = await conn.ExecuteScalarAsync<int>(chunkSql, new { key = "agent_create_chunk_size" });
            if (chunkSize <= 0) chunkSize = 1000;

            foreach (var task in tasks)
            {
                _logger.LogInformation("Processing TaskId={TaskId}, OrgId={OrgId}", task.Id, task.OrgId);

                // 4. Fetch Master Data for current Task (Inlined)
                var MasterEntries = await conn.QueryAsync<MasterTable>(
                    _mappingProvider.GetScriptForOperation("Master", "MasterEntries")?.Script,
                    new { orgId = task.OrgId ?? 0, EntryCategory = "AgentProfileMst" });

                var masterSql = _mappingProvider.GetScriptForOperation("Master", "KeyValueEntries")?.Script.Replace("{{FilterCriteria}}", 
                    MasterEntries.FirstOrDefault().FilterCriteria);

                //Fetch designation heirarchy
                var DesignationHeirarchy = await conn.QueryAsync<DesignationMaster>(
                    _mappingProvider.GetScriptForOperation("Master", "DesignationHeirarchy")?.Script,
                    new
                    {
                        orgId = orgId
                    });

                var ChannelMaster = await conn.QueryAsync<ChannelMaster>(
                    _mappingProvider.GetScriptForOperation("Master", "ChannelMaster")?.Script,
                    new
                    {
                        orgId = orgId
                    });

                var masterRows = await conn.QueryAsync<KeyValueEntry>(masterSql, new { orgId = task.OrgId ?? 0, masterName = "AgentProfileMst" });
                var agentClassDict = BuildLookup(masterRows.ToList(), "AGENT_CLASS");

                if (!File.Exists(task.FilePath))
                {
                    _logger.LogError("File not found: {FilePath}. Skipping TaskId={TaskId}", task.FilePath, task.Id);
                    continue;
                }

                // 5. Query Excel and process in chunks
                var rows = MiniExcel.Query<Agent>(task.FilePath);
                int rowCount = 0;
                int processedRows = 0;
                int rejectedRows = 0;
                List<string> successData = new();
                List<string> errorData = new();

                foreach (var batch in rows.Chunk(chunkSize))
                {
                    var batchList = batch.ToList();
                    foreach (var row in batchList)
                    {
                        if (string.IsNullOrWhiteSpace(row.AgentCode) && string.IsNullOrWhiteSpace(row.AgentName))
                        {
                            continue;
                        }
                        rowCount++;
                        row.OrgId = task.OrgId;
                        List<string> errors = new();
                        SanitizeStringProperties(row, errors);

                        // Validation logic
                        ValidateMaster(row.AgentClassDesc, agentClassDict, v => row.AgentClass = v, "Agent Class", errors);
                        if (!ValidateRow(row, validatorConfig.excelColumns, rowCount, out var validationErrors))
                            errors.AddRange(validationErrors);

                        row.Comments = errors.Any() ? "Rejected" : "Processed";
                        row.Reason = string.Join(" | ", errors);

                        if (errors.Any())
                        {
                            rejectedRows++;
                            errorData.Add(EncodeRowBase64(row));
                        }
                        else
                        {
                            processedRows++;
                            successData.Add(EncodeRowBase64(row));
                        }
                    }

                    // 6. Bulk Copy to Temp Table using GenericBulkOpsFactory (provider-agnostic)
                    var bulkSql = _mappingProvider.GetScriptForOperation("Agent", "BulkCopyTempAgent")?.Script;
                    if (string.IsNullOrWhiteSpace(bulkSql))
                        throw new Exception("Bulk COPY SQL for BulkCopyTempAgent missing");
                    var filteredBatch = batchList.Where(r => !string.IsNullOrWhiteSpace(r.AgentCode) || !string.IsNullOrWhiteSpace(r.AgentName)).ToList();
                    if (filteredBatch.Count == 0) continue;
                    await using var writer = await _bulkOpsFactory.BeginBinaryImportAsync(conn, bulkSql, token);
                    int index = 0;
                    foreach (var r in batchList)
                    {
                        index++;
                        writer.StartRow();
                        var currChannel = ChannelMaster.FirstOrDefault(x=> x.ChannelName.Equals(r.ChannelDesc));

                        var currDesignationHeirarchy = DesignationHeirarchy
                            .FirstOrDefault(x => x.ChannelId.Equals(currChannel?.ChannelId ?? -1000) && x.DesignationName.Equals(r.DesignationCodeDesc));
                        // 1-10 (pass same string/int values; provider wrapper will map types)

                        var agentCode = $"{currDesignationHeirarchy?.CodeFormat ?? "UNDEF"}{r.CreatedDate.ToString("yyMMddHH")}{index.ToString().PadLeft(3, '0')}";//eg: AG26021114001
                        writer.Write(r.AgentId.ToString());
                        writer.Write(agentCode);
                        writer.Write(r.AgentName ?? "");
                        writer.Write(r.BusinessName ?? "");
                        writer.Write(r.FirstName ?? "");
                        writer.Write(r.MiddleName ?? "");
                        writer.Write(r.LastName ?? "");
                        writer.Write(r.Prefix ?? "");
                        writer.Write(r.Suffix ?? "");
                        writer.Write(r.DoB?.ToString("yyyy-MM-dd") ?? "");

                        // 11-20
                        writer.Write(r.Nationality ?? "");
                        writer.Write(r.PreferredLanguage ?? "");
                        writer.Write(r.AgentLevel ?? "");
                        writer.Write(r.StaffCode ?? "");
                        writer.Write(r.Supervisor_Code ?? "");
                        writer.Write(r.ContractedDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.AgentStatusCode ?? "");
                        writer.Write(r.StatusDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.IsLicensed.ToString());
                        writer.Write(r.MaskedPanNumber ?? "");

                        // 21-30
                        writer.Write(r.AadhaarNumber ?? "");
                        writer.Write(r.IrdaLicenseNumber ?? "");
                        writer.Write(r.GstNumber ?? "");
                        writer.Write(r.CreatedBy ?? "");
                        writer.Write(r.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.Write(r.ModifiedBy ?? "");
                        writer.Write(r.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                        writer.Write(r.RowVersion?.ToString() ?? "");
                        writer.Write(r.IsActive.ToString());
                        writer.Write(r.ApplicationDocketNo ?? "");

                        // 31-40
                        writer.Write(r.Father_Husband_Nm ?? "");
                        writer.Write(r.EmployeeCode ?? "");
                        writer.Write(r.StartDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.PanAadharLinkFlag.ToString());
                        writer.Write(r.Sec206abFlag.ToString());
                        writer.Write(r.PackageID ?? "");
                        writer.Write(r.TaxStatus ?? "");
                        writer.Write(r.StateEid ?? "");
                        writer.Write(r.URN ?? "");
                        writer.Write(r.AdditionalComment ?? "");

                        // 41-50
                        writer.Write(r.AppointmentDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.IncorporationDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.CnctPersonDesig ?? "");
                        writer.Write(r.CnctPersonMobileNo ?? "");
                        writer.Write(r.CnctPersonEmail ?? "");
                        writer.Write(r.CnctPersonName ?? "");
                        writer.Write(r.CMSAgentType ?? "");
                        writer.Write(r.ServiceTaxNo ?? "");
                        writer.Write(r.UlipFlag.ToString());
                        writer.Write(r.TrainingGroupType ?? "");

                        // 51-60
                        writer.Write(r.Ifs ?? "");
                        writer.Write(r.RefresherTrainingCompleted.ToString());
                        writer.Write(r.IsMigrated.ToString());
                        writer.Write(r.MainPartnerClientCode ?? "");
                        writer.Write(r.AgentMainCodeVWEid ?? "");
                        writer.Write(r.RegistrationDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.Vertical ?? "");
                        writer.Write(r.BranchCode ?? "");
                        writer.Write(r.BranchName ?? "");
                        writer.Write(r.IC36TrngCompletionDate?.ToString("yyyy-MM-dd") ?? "");

                        // 61-70
                        writer.Write(r.STrngCompletionDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.ConfirmationDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.FgRockStarTrainingDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.IncrementDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LastPromotionDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.HRDoj?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.FgValueTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.HSecPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.ITSecPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.NPSTrngCompletionDate?.ToString("yyyy-MM-dd") ?? "");

                        // 71-80
                        writer.Write(r.WhistleBlowerTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.GovPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.InductionTrngDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LastWorkingDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LicenseNo ?? "");
                        writer.Write(r.LicenseType ?? "");
                        writer.Write(r.LicenseIssueDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.LicenseStatus ?? "");
                        writer.Write(r.AddressLine1 ?? "");

                        // 81-90
                        writer.Write(r.AddressLine2 ?? "");
                        writer.Write(r.AddressLine3 ?? "");
                        writer.Write(r.City ?? "");
                        writer.Write(r.StateDesc ?? "");
                        writer.Write(r.Pin ?? "");
                        writer.Write(r.Landmark ?? "");
                        writer.Write(r.Comments ?? "");
                        writer.Write(r.Reason ?? "");
                        writer.Write(r.OrgId);
                        writer.Write(r.AgentClassDesc ?? "");

                        // 91-100
                        writer.Write(r.BankAccTypeDesc ?? "");
                        writer.Write(r.GenderDesc ?? "");
                        writer.Write(r.TitleDesc ?? "");
                        writer.Write(r.ChannelDesc ?? "");
                        writer.Write(r.SubChannelDesc ?? "");
                        writer.Write(r.OccupationDesc ?? "");
                        writer.Write(r.AgentTypeCatDesc ?? "");
                        writer.Write(r.MaritalStatusDesc ?? "");
                        writer.Write(r.EducationDesc ?? "");
                        writer.Write(r.StateDesc ?? "");

                        // 101-110
                        writer.Write(r.CountryDesc ?? "");
                        writer.Write(r.DesignationCodeDesc ?? "");
                        writer.Write(r.LocationCodeDesc ?? "");
                        writer.Write(r.AgentTypeCodeDesc ?? "");
                        writer.Write(r.AgentSubTypeCodeDesc ?? "");
                        writer.Write(r.CandidateTypeDesc ?? "");
                        writer.Write(r.CommissionClassDesc ?? "");
                        writer.Write(r.AgentTypeDesc ?? "");
                        writer.Write(r.NomineeName ?? "");
                        writer.Write(r.Relationship ?? "");

                        // 111-120
                        writer.Write(r.PercentageShare.ToString());
                        writer.Write(r.NomineeAge.ToString());
                        writer.Write(r.AccountHolderName ?? "");
                        writer.Write(r.AccountNumber ?? "");
                        writer.Write(r.IFSC ?? "");
                        writer.Write(r.MICR ?? "");
                        writer.Write(r.BankName ?? "");
                        writer.Write(r.BankAccBranchName ?? "");
                        writer.Write(r.AccountType.ToString());
                        writer.Write(r.ActiveSince?.ToString("yyyy-MM-dd") ?? "");

                        // 121-130
                        writer.Write(r.FactoringHouse ?? "");
                        writer.Write(r.PreferredPaymentMode.ToString());
                        writer.Write(r.DoB?.ToString("yyyy-MM-dd") ?? "");
                        writer.Write(r.PanNumber ?? "");
                        writer.Write(r.Email ?? "");
                        writer.Write(r.MobileNo ?? "");
                        writer.Write(r.WorkContactNo ?? "");
                        writer.Write(r.ResidenceContactNo ?? "");
                        writer.Write(r.BloodGroup ?? "");
                        writer.Write(r.BirthPlace ?? "");

                        // 131-136
                        writer.Write(r.MaritalStatusDesc ?? "");
                        writer.Write(r.EducationCode?.ToString() ?? "");
                        writer.Write(r.EducationLevel ?? "");
                        writer.Write(r.WorkProfile ?? "");
                        writer.Write(r.AnnualIncome?.ToString() ?? "");
                        writer.Write(r.WorkExpMonths?.ToString() ?? "");

                        writer.Write(r.BranchDesc ?? "");

                    }

                    await writer.CompleteAsync(token);
                }

                //// 7. Finalize and Move from Temp to Main Tables (Inlined)
                var finalizeSql = _mappingProvider.GetScriptForOperation("Agent", "FinalizeAgent")?.Script;
                using (var tx = await conn.BeginTransactionAsync(token))
                {
                    await conn.ExecuteAsync(finalizeSql, transaction: tx);
                    await tx.CommitAsync(token);
                }

                var updateSql = _mappingProvider.GetScriptForOperation("Agent", "UpdateTask")?.Script
                    ?? throw new Exception("SQL for UpdateTask missing");
                var status = rejectedRows > 0 ? "CompletedWithErrors" : "Completed";
                await conn.ExecuteAsync(updateSql, new
                {
                    task.Id,
                    TotalRows = rowCount,
                    RowsProcessed = processedRows,
                    RowsRejected = rejectedRows,
                    ErrorMessage = errorData.Count > 0 ? string.Join(Environment.NewLine, errorData) : null,
                    SuccessData = successData.Count > 0 ? string.Join(Environment.NewLine, successData) : null,
                    Status = status
                });

                _logger.LogInformation("Completed TaskId={TaskId}", task.Id);
            }

            _logger.LogInformation("AgentCreateExcel job finished");
        }

        #region Helper Methods

        private InputExcelValidator LoadValidatorConfig()
        {
            var fileService = new FileService(_baseDir);
            var body = fileService.GetTemplate(Path.Combine("InputStructures"), "excelStructureValidator.json");

            return JsonSerializer.Deserialize<InputExcelValidator>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }

        private static Dictionary<string, int> BuildLookup(List<KeyValueEntry> list, string category)
        {
            return list
                .Where(x => x.EntryCategory == category && !string.IsNullOrWhiteSpace(x.EntryDesc))
                .GroupBy(x => x.EntryDesc.Trim().ToLower())
                .ToDictionary(g => g.Key, g => g.First().EntryIdentity);
        }

        private static void ValidateMaster(string? descValue, Dictionary<string, int> lookup, Action<int> assign, string fieldName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(descValue))
                return;

            if (lookup.TryGetValue(descValue.Trim().ToLower(), out var id))
            {
                assign(id);
            }
            else
            {
                errors.Add($"Invalid {fieldName}");
            }
        }

        private bool ValidateRow(Agent row, Excelcolumn[] rules, int rowNum, out List<string> errors)
        {
            errors = new List<string>();
            bool isValid = true;
            var type = typeof(Agent);

            foreach (var rule in rules)
            {
                var prop = type.GetProperty(rule.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) continue;

                var val = prop.GetValue(row)?.ToString();
                if (rule.TrimContent) val = val?.Trim();

                // 1. Required Check
                if (!rule.AllowBlank && string.IsNullOrWhiteSpace(val))
                {
                    errors.Add($"Col '{rule.ColumnName}' is required.");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(val)) continue;

                // 2. Regex Check
                if (rule.UseRegEx && !Regex.IsMatch(val, rule.DataFormat))
                {
                    errors.Add($"Col '{rule.ColumnName}' has invalid format.");
                    isValid = false;
                }
            }
            return isValid;
        }

        private static void SanitizeStringProperties(Agent row, List<string> errors)
        {
            foreach (var prop in typeof(Agent).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType != typeof(string))
                {
                    continue;
                }

                var value = (string?)prop.GetValue(row);
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                var sanitized = value.Replace("\0", string.Empty);
                if (!string.Equals(value, sanitized, StringComparison.Ordinal))
                {
                    errors.Add($"Col '{prop.Name}' contains invalid characters.");
                    prop.SetValue(row, sanitized);
                }
                }
        }

        private static string EncodeRowBase64(Agent row)
        {
            var json = JsonSerializer.Serialize(row);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
        #endregion
    }
}
