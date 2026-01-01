using CommonLibrary;
using MiniExcelLibs;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tasks.Models;

namespace Tasks.Insurance
{
    public class AgentCreateExcel
    {
        private readonly string _connectionString;
        private readonly FileService _fileService;
        private readonly string _baseDir = AppContext.BaseDirectory;

        public AgentCreateExcel()
        {
            _connectionString =
                "server=ep-silent-silence-a1fanpxl-pooler.ap-southeast-1.aws.neon.tech;" +
                "username=neondb_owner;password=npg_MPXYuy4jTe1r;database=neondb;";
            _fileService = new FileService(_baseDir);
        }
        public async Task ProcessAgentCreateData()
        {
            var validatorConfig = LoadValidatorConfig();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var tasks = await GetPendingTasksAsync(conn);
            if (!tasks.Any()) return;

            int chunkSize = await GetConfigValueAsync(conn, "agent_create_chunk_size", 1000);

            foreach (var task in tasks)
            {
                try
                {
                    await ProcessSingleFileAsync(task, conn, validatorConfig, chunkSize);
                    //await FinalizeDataTransfer(conn, task.OrgId ?? 0);
                    Console.WriteLine($"Successfully processed: {task.FilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {task.FilePath}: {ex.Message}");
                }
            }
        }
        private static async Task<List<FileProcessingTask>> GetPendingTasksAsync(NpgsqlConnection conn)
        {
            List<FileProcessingTask> tasks = new();

            using var cmd = new NpgsqlCommand(
                @"SELECT filepath, orgid 
          FROM hms.fileprocessingtasks 
          WHERE status = 'Pending';", conn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string dbPath = reader.GetString(0);

                string fullPath = Path.Combine(AppContext.BaseDirectory, "FileProcess", Path.GetFileName(dbPath));

                tasks.Add(new FileProcessingTask
                {
                    FilePath = fullPath,
                    OrgId = reader.GetInt32(1)
                });
            }

            return tasks;
        }
        private async Task ProcessSingleFileAsync(FileProcessingTask task, NpgsqlConnection conn, InputExcelValidator config, int chunkSize)
        {
            // Prepare lookups for master data validation
            var masterData = await GetMasterDataAsync(conn, task.OrgId ?? 0, "AgentProfileMst");
            var agentClassDict = BuildLookup(masterData, "AGENT_CLASS");
            string directoryPath = Path.GetDirectoryName(task.FilePath);

            // Create the directory if it doesn't exist in the bin folder
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Created missing directory: {directoryPath}");
            }

            // Check if the file actually exists before processing
            if (!File.Exists(task.FilePath))
            {
                throw new FileNotFoundException($"The Excel file was not found at: {task.FilePath}");
            }
            // Open stream using MiniExcel (Streaming avoids loading whole file into RAM)
            var rows = MiniExcel.Query<AgentDto>(task.FilePath);
            int rowCount = 0;

            // Process in Chunks
            foreach (var batch in rows.Chunk(chunkSize))
            {
                foreach (var row in batch)
                {
                    rowCount++;
                    row.OrgId = task.OrgId;

                    // Validation Logic
                    List<string> errors = new();
                    ValidateMaster(row.AgentClassDesc, agentClassDict, v => row.AgentClass = v, "Agent Class", errors);

                    if (!ValidateRow(row, config.excelColumns, rowCount, out var validationErrors))
                        errors.AddRange(validationErrors);

                    row.Comments = errors.Any() ? "Rejected" : "Processed";
                    row.Reason = string.Join(" | ", errors);
                }

                await BulkImportBinary(batch.ToList(), conn);
            }
        }
        private async Task BulkImportBinary(List<AgentDto> rows, NpgsqlConnection conn)
        {
            // The SQL column list must stay in the EXACT order of your writer.WriteAsync calls
            string copySql = @"
        COPY hms.tempagentdto (
            AgentId, AgentCode, AgentName, BusinessName, FirstName, MiddleName, LastName, Prefix,
            Suffix, DOB, Nationality, PreferredLanguage, AgentLevel, StaffCode, Supervisor_Code, 
            ContractedDate, AgentStatusCode, StatusDate, IsLicensed, MaskedPanNumber, aadhaar_number,
            IrdaLicenseNumber, GstNumber, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, 
            RowVersion, IsActive, ApplicationDocketNo, Father_Husband_Nm, EmployeeCode, StartDate, 
            PanAadharLinkFlag, Sec206abFlag, PackageID, TaxStatus, StateEid, URN, AdditionalComment, 
            AppointmentDate, IncorporationDate, CnctPersonDesig, CnctPersonMobileNo, CnctPersonEmail, 
            CnctPersonName, CMSAgentType, ServiceTaxNo, UlipFlag, TrainingGroupType, Ifs, 
            RefresherTrainingCompleted, IsMigrated, MainPartnerClientCode, AgentMaincodevwEid, 
            RegistrationDate, Vertical, BranchCode, BranchName, Ic36TrngCompletionDate, 
            STrngCompletionDate, ConfirmationDate, FgRockstarTrainingDate, IncrementDate, 
            LastPromotionDate, HRDoj, FgValueTrngDate, HSecPolicyTrngDate, ItSecPolicyTrngDate, 
            NpsTrngCompletionDate, WhistleBlowerTrngDate, GovPolicyTrngDate, InductionTrngDate, 
            LastWorkingDate, LicenseNo, LicenseType, LicenseIssueDate, LicenseExpiryDate, 
            LicenseStatus, AddressLine1, AddressLine2, AddressLine3, City, PIN, Landmark, 
            Comments, Reason, orgid, agent_class_desc, bank_acc_type_desc, gender_desc, 
            title_desc, channel_desc, sub_channel_desc, occupation_desc, agent_type_cat_desc, 
            marital_status_desc, education_desc, state_desc, country_desc, designation_code_desc, 
            location_code_desc, agent_type_code_desc, agent_sub_type_code_desc, candidate_type_desc, 
            commission_class_desc, agent_type_desc, NomineeName, Relationship, PercentageShare, 
            NomineeAge, accountholdername, accountnumber, ifsc, micr, bankname, Accbranchname, 
            accounttype, activesince, factoringhouse, preferredpaymentmode, dateofbirth, 
            pannumber, email, mobileno, workcontactno, residencecontactno, bloodgroup, 
            birthplace, martial_status_desc, pinfo_education_desc, educationlevel, workprofile, 
            annualincome, workexpmonths
        ) FROM STDIN (FORMAT BINARY);";

            using (var writer = await conn.BeginBinaryImportAsync(copySql))
            {
                foreach (var r in rows)
                {
                    await writer.StartRowAsync();

                    // All writes MUST use NpgsqlDbType.Varchar and be converted to strings
                    await writer.WriteAsync(r.AgentId.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BusinessName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.FirstName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MiddleName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LastName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Prefix, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Suffix, NpgsqlDbType.Varchar);

                    // Handle Nullable DateTime conversion
                    await writer.WriteAsync(r.DOB?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Nationality, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.PreferredLanguage, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentLevel, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.StaffCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Supervisor_Code, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ContractedDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentStatusCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.StatusDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);

                    // Handle Boolean to String
                    await writer.WriteAsync(r.IsLicensed.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MaskedPanNumber, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.aadhaar_number, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IrdaLicenseNumber, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.GstNumber, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CreatedBy, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ModifiedBy, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss"), NpgsqlDbType.Varchar);

                    // Handle Nullable Int to String
                    await writer.WriteAsync(r.RowVersion?.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IsActive.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ApplicationDocketNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Father_Husband_Nm, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.EmployeeCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.StartDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.PanAadharLinkFlag.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Sec206abFlag.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.PackageID, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.TaxStatus, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.StateEid, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.URN, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AdditionalComment, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AppointmentDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IncorporationDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CnctPersonDesig, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CnctPersonMobileNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CnctPersonEmail, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CnctPersonName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CMSAgentType, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ServiceTaxNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.UlipFlag.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.TrainingGroupType, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Ifs, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.RefresherTrainingCompleted.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IsMigrated.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MainPartnerClientCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentMaincodevwEid, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.RegistrationDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Vertical, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BranchCode, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BranchName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Ic36TrngCompletionDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.STrngCompletionDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ConfirmationDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.FgRockstarTrainingDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IncrementDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LastPromotionDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.HRDoj?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.FgValueTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.HSecPolicyTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ItSecPolicyTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.NpsTrngCompletionDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.WhistleBlowerTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.GovPolicyTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.InductionTrngDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LastWorkingDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LicenseNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LicenseType, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LicenseIssueDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LicenseExpiryDate?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LicenseStatus, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AddressLine1, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AddressLine2, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AddressLine3, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.City, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Pin, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Landmark, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Comments, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Reason, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.OrgId?.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentClassDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BankAccTypeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.GenderDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.TitleDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ChannelDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.SubChannelDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.OccupationDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentTypeCatDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MaritalStatusDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.EducationDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.StateDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CountryDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.DesignationCodeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.LocationCodeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentTypeCodeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentSubTypeCodeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CandidateTypeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.CommissionClassDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AgentTypeDesc, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.NomineeName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Relationship, NpgsqlDbType.Varchar);

                    // Handle Decimal to String
                    await writer.WriteAsync(r.PercentageShare.ToString(), NpgsqlDbType.Varchar);

                    // Handle Long to String
                    await writer.WriteAsync(r.NomineeAge.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AccountHolderName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AccountNumber, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.IFSC, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MICR, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BankName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BankAccBranchName, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AccountType.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ActiveSince?.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.FactoringHouse, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.PreferredPaymentMode.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.DateOfBirth.ToString("yyyy-MM-dd"), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.PanNumber, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.Email, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MobileNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.WorkContactNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.ResidenceContactNo, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BloodGroup, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.BirthPlace, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.MartialStatus, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.EducationCode?.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.EducationLevel, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.WorkProfile, NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.AnnualIncome?.ToString(), NpgsqlDbType.Varchar);
                    await writer.WriteAsync(r.WorkExpMonths?.ToString(), NpgsqlDbType.Varchar);
                }
                await writer.CompleteAsync();
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

        private InputExcelValidator LoadValidatorConfig()
        {
            var json = _fileService.GetTemplate("InputStructures", "excelStructureValidator.json");
            return JsonSerializer.Deserialize<InputExcelValidator>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<int> GetConfigValueAsync(NpgsqlConnection conn, string key, int defaultValue)
        {
            using var cmd = new NpgsqlCommand("SELECT config_value FROM hms.api_config WHERE config_key = @key LIMIT 1", conn);
            cmd.Parameters.AddWithValue("key", key);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? int.Parse(result.ToString()) : defaultValue;
        }
        private static async Task<List<KeyValueEntry>> GetMasterDataAsync(NpgsqlConnection conn, int orgId, string masterName)
        {
            var list = new List<KeyValueEntry>();

            var sql = @"
             SELECT
                    k.orgid,
                    k.entrycategory,
                    k.entryidentity,
                    k.entrydesc
                    FROM hmsmaster.mastertables m
                    JOIN hmsmaster.keyvalueentries k
                        ON k.orgid = m.orgid
                    WHERE m.orgid = @orgId
                    AND m.entrycategory = @masterName
                    AND k.entrycategory IN (
                          SELECT trim(both '''' from trim(x))
                          FROM unnest(
                              string_to_array(
                                  regexp_replace(
                                      m.filtercriteria,
                                      '.*IN\s*\(|\)',
                                      '',
                                      'gi'
                                  ),
                                  ','
                              )
                          ) AS x
                    );
    ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@orgId", orgId);
            cmd.Parameters.AddWithValue("@masterName", masterName);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new KeyValueEntry
                {
                    orgid = reader.GetInt32(0),
                    EntryCategory = reader.GetString(1),
                    EntryIdentity = reader.GetInt32(2),
                    EntryDesc = reader.GetString(3)
                });
            }

            return list;
        }

        private static Dictionary<string, int> BuildLookup(List<KeyValueEntry> list, string category)
        {
            return list
                .Where(x => x.EntryCategory == category && !string.IsNullOrWhiteSpace(x.EntryDesc))
                .ToDictionary(
                    x => x.EntryDesc.Trim().ToLower(),
                    x => x.EntryIdentity
                );
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
    }
}