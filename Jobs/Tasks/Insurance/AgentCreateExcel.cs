using CommonLibrary;
using CommonLibrary.mapping;
using Dapper;
using MiniExcelLibs;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using Repository;
using System.Reflection;
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
        public AgentCreateExcel(IJobExecutionContext jobExecutionContext,
            IMappingProvider mappingProvider, 
            Microsoft.Extensions.Configuration.IConfiguration configuration, 
            ILogger<AgentCreateExcel> logger,
            IConnectionScope connectionScope)
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
        }

        public async Task ProcessAgentCreateData()
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
                var masterSql = _mappingProvider.GetScriptForOperation("Agent", "GetMasterData")?.Script;
                var masterRows = await conn.QueryAsync<KeyValueEntry>(masterSql, new { orgId = task.OrgId ?? 0, masterName = "AgentProfileMst" });
                var agentClassDict = BuildLookup(masterRows.ToList(), "AGENT_CLASS");

                if (!File.Exists(task.FilePath))
                {
                    _logger.LogError("File not found: {FilePath}. Skipping TaskId={TaskId}", task.FilePath, task.Id);
                    continue;
                }

                // 5. Query Excel and process in chunks
                var rows = MiniExcel.Query<AgentDto>(task.FilePath);
                int rowCount = 0;

                foreach (var batch in rows.Chunk(chunkSize))
                {
                    var batchList = batch.ToList();
                    foreach (var row in batchList)
                    {
                        rowCount++;
                        row.OrgId = task.OrgId;
                        List<string> errors = new();

                        // Validation logic
                        ValidateMaster(row.AgentClassDesc, agentClassDict, v => row.AgentClass = v, "Agent Class", errors);
                        if (!ValidateRow(row, validatorConfig.excelColumns, rowCount, out var validationErrors))
                            errors.AddRange(validationErrors);

                        row.Comments = errors.Any() ? "Rejected" : "Processed";
                        row.Reason = string.Join(" | ", errors);
                    }

                    // 6. Bulk Copy to Temp Table (Inlined)
                    var bulkSql = _mappingProvider.GetScriptForOperation("Agent", "BulkCopyTempAgent")?.Script;
                    if (conn is NpgsqlConnection npgsqlConn)
                    {
                        using (var writer = await npgsqlConn.BeginBinaryImportAsync(bulkSql))
                        {
                            foreach (var r in batchList)
                            {
                                await writer.StartRowAsync();
                                // 1-10
                                await writer.WriteAsync(r.AgentId.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BusinessName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.FirstName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MiddleName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LastName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Prefix ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Suffix ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.DOB?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);

                                // 11-20
                                await writer.WriteAsync(r.Nationality ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.PreferredLanguage ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentLevel ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StaffCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Supervisor_Code ?? "", NpgsqlDbType.Varchar); // Maps to supervisor_code
                                await writer.WriteAsync(r.ContractedDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentStatusCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StatusDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IsLicensed.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MaskedPanNumber ?? "", NpgsqlDbType.Varchar);

                                // 21-30
                                await writer.WriteAsync(r.aadhaar_number ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IrdaLicenseNumber ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.GstNumber ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CreatedBy ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ModifiedBy ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.RowVersion?.ToString() ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IsActive.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ApplicationDocketNo ?? "", NpgsqlDbType.Varchar);

                                // 31-40
                                await writer.WriteAsync(r.Father_Husband_Nm ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.EmployeeCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StartDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.PanAadharLinkFlag.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Sec206abFlag.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.PackageID ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.TaxStatus ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StateEid ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.URN ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AdditionalComment ?? "", NpgsqlDbType.Varchar);

                                // 41-50
                                await writer.WriteAsync(r.AppointmentDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IncorporationDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CnctPersonDesig ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CnctPersonMobileNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CnctPersonEmail ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CnctPersonName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CMSAgentType ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ServiceTaxNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.UlipFlag.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.TrainingGroupType ?? "", NpgsqlDbType.Varchar);

                                // 51-60
                                await writer.WriteAsync(r.Ifs ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.RefresherTrainingCompleted.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IsMigrated.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MainPartnerClientCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentMaincodevwEid ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.RegistrationDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Vertical ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BranchCode ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BranchName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Ic36TrngCompletionDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);

                                // 61-70
                                await writer.WriteAsync(r.STrngCompletionDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ConfirmationDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.FgRockstarTrainingDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IncrementDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LastPromotionDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.HRDoj?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.FgValueTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.HSecPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ItSecPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.NpsTrngCompletionDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);

                                // 71-80
                                await writer.WriteAsync(r.WhistleBlowerTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.GovPolicyTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.InductionTrngDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LastWorkingDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LicenseNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LicenseType ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LicenseIssueDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LicenseExpiryDate?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LicenseStatus ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AddressLine1 ?? "", NpgsqlDbType.Varchar);

                                // 81-90
                                await writer.WriteAsync(r.AddressLine2 ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AddressLine3 ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.City ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StateDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Pin ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Landmark ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Comments ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Reason ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.OrgId, NpgsqlDbType.Integer);
                                await writer.WriteAsync(r.AgentClassDesc ?? "", NpgsqlDbType.Varchar);

                                // 91-100
                                await writer.WriteAsync(r.BankAccTypeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.GenderDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.TitleDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ChannelDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.SubChannelDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.OccupationDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentTypeCatDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MaritalStatusDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.EducationDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.StateDesc ?? "", NpgsqlDbType.Varchar);

                                // 101-110
                                await writer.WriteAsync(r.CountryDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.DesignationCodeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.LocationCodeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentTypeCodeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentSubTypeCodeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CandidateTypeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.CommissionClassDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AgentTypeDesc ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.NomineeName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Relationship ?? "", NpgsqlDbType.Varchar);

                                // 111-120
                                await writer.WriteAsync(r.PercentageShare.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.NomineeAge.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AccountHolderName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AccountNumber ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.IFSC ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MICR ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BankName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BankAccBranchName ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AccountType.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ActiveSince?.ToString("yyyy-MM-dd") ?? "", NpgsqlDbType.Varchar);

                                // 121-130
                                await writer.WriteAsync(r.FactoringHouse ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.PreferredPaymentMode.ToString(), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.DateOfBirth.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.PanNumber ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.Email ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.MobileNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.WorkContactNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.ResidenceContactNo ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BloodGroup ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.BirthPlace ?? "", NpgsqlDbType.Varchar);

                                // 131-136
                                await writer.WriteAsync(r.MartialStatus ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.EducationCode?.ToString() ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.EducationLevel ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.WorkProfile ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.AnnualIncome?.ToString() ?? "", NpgsqlDbType.Varchar);
                                await writer.WriteAsync(r.WorkExpMonths?.ToString() ?? "", NpgsqlDbType.Varchar);
                            }
                            await writer.CompleteAsync();
                        }
                    }
                }

                // 7. Finalize and Move from Temp to Main Tables (Inlined)
                var finalizeSql = _mappingProvider.GetScriptForOperation("Agent", "FinalizeAgent")?.Script;
                using (var tx = await conn.BeginTransactionAsync(token))
                {
                    await conn.ExecuteAsync(finalizeSql, transaction: tx);
                    await tx.CommitAsync(token);
                }

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

        private bool ValidateRow(AgentDto row, Excelcolumn[] rules, int rowNum, out List<string> errors)
        {
            errors = new List<string>();
            bool isValid = true;
            var type = typeof(AgentDto);

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
        #endregion
    }
}
