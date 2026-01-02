using Quartz;

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
        private readonly IJobExecutionContext _jobExecutionContext;
        private int orgId = 0;
        public JobKey jobKey;
        private readonly string _connectionString;
        private readonly FileService _fileService;
        private readonly string _baseDir = AppContext.BaseDirectory;
        private readonly ILogger<AgentCreateExcel> _logger;
        public AgentCreateExcel(IJobExecutionContext jobExecutionContext, ILogger<AgentCreateExcel> logger)
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
            _connectionString =
               "server=ep-silent-silence-a1fanpxl-pooler.ap-southeast-1.aws.neon.tech;" +
               "username=neondb_owner;password=npg_MPXYuy4jTe1r;database=neondb;";
            _fileService = new FileService(_baseDir);
            _logger = logger;
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
                    _logger.LogInformation("Excel Data Successfully Inserted Into TempTable");

                    await FinalizeDataTransfer(conn);
                    _logger.LogInformation("Data Successfully processed");

                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Error processing {task.FilePath}: {ex.Message}");
                }
            }
        }
        private static async Task<List<FileProcessingTask>> GetPendingTasksAsync(NpgsqlConnection conn)
        {
            List<FileProcessingTask> tasks = new();

            using var cmd = new NpgsqlCommand(@"SELECT filepath, orgid FROM hms.fileprocessingtasks WHERE status = 'Pending';", conn);

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
            var masterData = await GetMasterDataAsync(conn, task.OrgId ?? 0, "AgentProfileMst");
            var agentClassDict = BuildLookup(masterData, "AGENT_CLASS");
            
            var rows = MiniExcel.Query<AgentDto>(task.FilePath);
            int rowCount = 0;

            foreach (var batch in rows.Chunk(chunkSize))
            {
                foreach (var row in batch)
                {
                    rowCount++;
                    row.OrgId = task.OrgId;

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
            string copySql = @"
        COPY hms.tempagentdto (
            agentid, agentcode, agentname, businessname, firstname, middlename, lastname, prefix, suffix, dob,
            nationality, preferredlanguage, agentlevel, staffcode, supervisor_code, contracteddate, agentstatuscode, statusdate, islicensed, maskedpannumber,
            aadhaar_number, irdalicensenumber, gstnumber, createdby, createddate, modifiedby, modifieddate, rowversion, isactive, applicationdocketno,
            father_husband_nm, employeecode, startdate, panaadharlinkflag, sec206abflag, packageid, taxstatus, stateeid, urn, additionalcomment,
            appointmentdate, incorporationdate, cnctpersondesig, cnctpersonmobileno, cnctpersonemail, cnctpersonname, cmsagenttype, servicetaxno, ulipflag, traininggrouptype,
            ifs, refreshertrainingcompleted, ismigrated, mainpartnerclientcode, agentmaincodevweid, registrationdate, vertical, branchcode, branchname, ic36trngcompletiondate,
            strngcompletiondate, confirmationdate, fgrockstartrainingdate, incrementdate, lastpromotiondate, hrdoj, fgvaluetrngdate, hsecpolicytrngdate, itsecpolicytrngdate, npstrngcompletiondate,
            whistleblowertrngdate, govpolicytrngdate, inductiontrngdate, lastworkingdate, licenseno, licensetype, licenseissuedate, licenseexpirydate, licensestatus, addressline1,
            addressline2, addressline3, city, state, pin, landmark, comments, reason, orgid, agent_class_desc,
            bank_acc_type_desc, gender_desc, title_desc, channel_desc, sub_channel_desc, occupation_desc, agent_type_cat_desc, marital_status_desc, education_desc, state_desc,
            country_desc, designation_code_desc, location_code_desc, agent_type_code_desc, agent_sub_type_code_desc, candidate_type_desc, commission_class_desc, agent_type_desc, nomineename, relationship,
            percentageshare, nomineeage, accountholdername, accountnumber, ifsc, micr, bankname, accbranchname, accounttype, activesince,
            factoringhouse, preferredpaymentmode, dateofbirth, pannumber, email, mobileno, workcontactno, residencecontactno, bloodgroup, birthplace,
            martial_status_desc, pinfo_education_desc, educationlevel, workprofile, annualincome, workexpmonths
        ) FROM STDIN (FORMAT BINARY);";

            using (var writer = await conn.BeginBinaryImportAsync(copySql))
            {
                foreach (var r in rows)
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

        private async Task FinalizeDataTransfer(NpgsqlConnection conn)
        {
            _logger.LogInformation("FinalizeDataTransfer starting.");

            string insertSql = @"WITH master_map AS (
                                        SELECT
                                            t.agentcode,
                                            t.orgid,
                                    
                                            MAX(CASE WHEN k.entrycategory = 'BANK_ACC_TYP'        THEN k.entryidentity END) AS bankacctype,
                                            MAX(CASE WHEN k.entrycategory = 'CHANNEL_NAME'        THEN k.entryidentity END) AS channel,
                                            MAX(CASE WHEN k.entrycategory = 'SUB_CHANNEL'         THEN k.entryidentity END) AS subchannel,
                                            MAX(CASE WHEN k.entrycategory = 'AGENT_TYPE_CAT'      THEN k.entryidentity END) AS agent_type_cat,
                                            MAX(CASE WHEN k.entrycategory = 'AGENT_CLASS'         THEN k.entryidentity END) AS agent_class,
                                            MAX(CASE WHEN k.entrycategory = 'MARITAL_STATUS'      THEN k.entryidentity END) AS marital_status,
                                            MAX(CASE WHEN k.entrycategory = 'EDUCATION_CODE'      THEN k.entryidentity END) AS education,
                                            MAX(CASE WHEN k.entrycategory = 'STATE_NAME'          THEN k.entryidentity END) AS state,
                                            MAX(CASE WHEN k.entrycategory = 'COUNTRY'             THEN k.entryidentity END) AS country,
                                            MAX(CASE WHEN k.entrycategory = 'GENDER'              THEN k.entryidentity END) AS gender,
                                            MAX(CASE WHEN k.entrycategory = 'TITLE'               THEN k.entryidentity END) AS title,
                                            MAX(CASE WHEN k.entrycategory = 'OCCUPATION'          THEN k.entryidentity END) AS occupation,
                                            MAX(CASE WHEN k.entrycategory = 'AGENT_SUB_TYPE_CODE' THEN k.entryidentity END) AS agent_sub_type_code,
                                            MAX(CASE WHEN k.entrycategory = 'DESIGNATION'         THEN k.entryidentity END) AS designation_code,
                                            MAX(CASE WHEN k.entrycategory = 'AGENT_TYPE_CODE'     THEN k.entryidentity END) AS agent_type_code,
                                            MAX(CASE WHEN k.entrycategory = 'LOCATION'            THEN k.entryidentity END) AS location_code,
                                            MAX(CASE WHEN k.entrycategory = 'CANDIDATE_TYP'       THEN k.entryidentity END) AS candidatetype,
                                            MAX(CASE WHEN k.entrycategory = 'AGNT_TYP'            THEN k.entryidentity END) AS agenttype,
                                            MAX(CASE WHEN k.entrycategory = 'COMMISSION_CLASS'    THEN k.entryidentity END) AS commissionclass,
                                    
                                            -- for Address (DESCRIPTIONS, not IDs)
                                            MAX(CASE WHEN k.entrycategory = 'STATE_NAME' THEN k.entrydesc END) AS state_desc,
                                            MAX(CASE WHEN k.entrycategory = 'COUNTRY'    THEN k.entrydesc END) AS country_desc
                                    
                                        FROM hms.tempagentdto t
                                        LEFT JOIN hmsmaster.keyvalueentries k
                                          ON k.orgid = t.orgid
                                         AND (
                                               (k.entrycategory = 'BANK_ACC_TYP'        AND k.entrydesc = t.bank_acc_type_desc)
                                            OR (k.entrycategory = 'CHANNEL_NAME'        AND k.entrydesc = t.channel_desc)
                                            OR (k.entrycategory = 'SUB_CHANNEL'         AND k.entrydesc = t.sub_channel_desc)
                                            OR (k.entrycategory = 'AGENT_TYPE_CAT'      AND k.entrydesc = t.agent_type_cat_desc)
                                            OR (k.entrycategory = 'AGENT_CLASS'         AND k.entrydesc = t.agent_class_desc)
                                            OR (k.entrycategory = 'MARITAL_STATUS'      AND k.entrydesc = t.marital_status_desc)
                                            OR (k.entrycategory = 'EDUCATION_CODE'      AND k.entrydesc = t.education_desc)
                                            OR (k.entrycategory = 'STATE_NAME'          AND k.entrydesc = t.state_desc)
                                            OR (k.entrycategory = 'COUNTRY'             AND k.entrydesc = t.country_desc)
                                            OR (k.entrycategory = 'GENDER'              AND k.entrydesc = t.gender_desc)
                                            OR (k.entrycategory = 'TITLE'               AND k.entrydesc = t.title_desc)
                                            OR (k.entrycategory = 'OCCUPATION'          AND k.entrydesc = t.occupation_desc)
                                            OR (k.entrycategory = 'AGENT_SUB_TYPE_CODE' AND k.entrydesc = t.agent_sub_type_code_desc)
                                            OR (k.entrycategory = 'DESIGNATION'         AND k.entrydesc = t.designation_code_desc)
                                            OR (k.entrycategory = 'AGENT_TYPE_CODE'     AND k.entrydesc = t.agent_type_code_desc)
                                            OR (k.entrycategory = 'LOCATION'            AND k.entrydesc = t.location_code_desc)
                                            OR (k.entrycategory = 'CANDIDATE_TYP'       AND k.entrydesc = t.candidate_type_desc)
                                            OR (k.entrycategory = 'AGNT_TYP'            AND k.entrydesc = t.agent_type_desc)
                                            OR (k.entrycategory = 'COMMISSION_CLASS'    AND k.entrydesc = t.commission_class_desc)
                                         )
                                        WHERE t.comments = 'Processed'
                                        GROUP BY t.agentcode, t.orgid
                                    ),
                                  ins AS (
                                    INSERT INTO hms.agent (
                                        agent_code, agent_name,
                                        business_name, first_name, middle_name, last_name, prefix,
                                        suffix, dob, nationality,
                                        preferred_language, agent_level,
                                        staff_code, contracted_date, agent_status_code, status_date,
                                        is_licensed, pan_number, aadhaar_number, irda_license_number, gst_number,
                                        created_by, created_date, modified_by, modified_date, rowversion,
                                        supervisor_id, is_active,
                                        applicationdocketno, father_husband_nm,
                                        employeecode, startdate, panaadharlinkflag, sec206abflag,
                                        taxstatus, urn,
                                        additionalcomment, appointmentdate, incorporationdate,
                                        cnctpersondesig, cnctpersonmobileno, cnctpersonemail, cnctpersonname,
                                        cmsagenttype, packageid, servicetaxno, ulipflag, traininggrouptype, ifs,
                                        refreshertrainingcompleted, ismigrated, mainpartnerclientcode,
                                        agentmaincodevweid, registrationdate,
                                        vertical, branchcode, branchname,
                                        ic36trngcompletiondate, strngcompletiondate, confirmationdate,
                                        fgrockstartrainingdate, incrementdate, lastpromotiondate, hrdoj,
                                        fgvaluetrngdate, hsecpolicytrngdate, itsecpolicytrngdate,
                                        npstrngcompletiondate, whistleblowertrngdate, govpolicytrngdate,
                                        inductiontrngdate, lastworkingdate,
                                        licenseno, licensetype, licenseissuedate, licenseexpirydate,
                                        licensestatus, orgid,
                                        bankacctype, channel, subchannel,
                                        agent_type_cat, agent_class, martial_status, education,
                                        state, country, gender, title, occupation,
                                        agent_sub_type_code, designation_code, agent_type_code,
                                        location_code, candidatetype, agenttype, commissionclass
                                    )
                                      SELECT
                                        t.agentcode, 
                                        t.agentname,
                                        t.businessname, 
                                        t.firstname, 
                                        t.middlename, 
                                        t.lastname, 
                                        t.prefix,
                                        t.suffix, 
                                        NULLIF(TRIM(t.dob), '')::date, 
                                        t.nationality,
                                        t.preferredlanguage,
                                        CASE TRIM(t.agentlevel)
                                            WHEN 'Level1' THEN 1 
                                            WHEN 'Level2' THEN 2 
                                            WHEN 'Level3' THEN 3 
                                            ELSE NULL 
                                        END,
                                        t.staffcode, 
                                        NULLIF(TRIM(t.contracteddate), '')::date, 
                                        t.agentstatuscode, 
                                        NULLIF(TRIM(t.statusdate), '')::date,
                                        CASE 
                                            WHEN t.islicensed IN ('Y','y','1','TRUE','true','T') THEN TRUE 
                                            WHEN t.islicensed IN ('N','n','0','FALSE','false','F') THEN FALSE 
                                            ELSE FALSE 
                                        END,
                                        t.maskedpannumber, 
                                        t.aadhaar_number, 
                                        t.irdalicensenumber, 
                                        t.gstnumber,
                                        t.createdby, 
                                        NULLIF(TRIM(t.createddate), '')::date, 
                                        t.modifiedby, 
                                        NULLIF(TRIM(t.modifieddate), '')::date,
                                        NULLIF(TRIM(t.rowversion), '')::integer,
                                        a.agent_id::integer,
                                        CASE 
                                            WHEN t.isactive IN ('Y','y','1','TRUE','true','T') THEN TRUE 
                                            WHEN t.isactive IN ('N','n','0','FALSE','false','F') THEN FALSE 
                                            ELSE FALSE 
                                        END,
                                        t.applicationdocketno, 
                                        t.father_husband_nm,
                                        t.employeecode, 
                                        NULLIF(TRIM(t.startdate), '')::date,
                                        CASE WHEN t.panaadharlinkflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
                                        CASE WHEN t.sec206abflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
                                        t.taxstatus, 
                                        t.urn,
                                        t.additionalcomment, 
                                        NULLIF(TRIM(t.appointmentdate), '')::date, 
                                        NULLIF(TRIM(t.incorporationdate), '')::date,
                                        t.cnctpersondesig, 
                                        t.cnctpersonmobileno, 
                                        t.cnctpersonemail, 
                                        t.cnctpersonname,
                                        t.cmsagenttype, 
                                        t.packageid, 
                                        t.servicetaxno,
                                        CASE WHEN t.ulipflag IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
                                        t.traininggrouptype, 
                                        t.ifs,
                                        CASE WHEN t.refreshertrainingcompleted IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
                                        CASE WHEN t.ismigrated IN ('Y','y','1','T','TRUE','true') THEN TRUE ELSE FALSE END,
                                        t.mainpartnerclientcode, 
                                        t.agentmaincodevweid, 
                                        NULLIF(TRIM(t.registrationdate), '')::date,
                                        t.vertical, 
                                        t.branchcode, 
                                        t.branchname, 
                                        NULLIF(TRIM(t.ic36trngcompletiondate), '')::date,
                                        NULLIF(TRIM(t.strngcompletiondate), '')::date, 
                                        NULLIF(TRIM(t.confirmationdate), '')::date,
                                        NULLIF(TRIM(t.fgrockstartrainingdate), '')::date, 
                                        NULLIF(TRIM(t.incrementdate), '')::date, 
                                        NULLIF(TRIM(t.lastpromotiondate), '')::date,
                                        NULLIF(TRIM(t.hrdoj), '')::date, 
                                        NULLIF(TRIM(t.fgvaluetrngdate), '')::date, 
                                        NULLIF(TRIM(t.hsecpolicytrngdate), '')::date,
                                        NULLIF(TRIM(t.itsecpolicytrngdate), '')::date, 
                                        NULLIF(TRIM(t.npstrngcompletiondate), '')::date, 
                                        NULLIF(TRIM(t.whistleblowertrngdate), '')::date,
                                        NULLIF(TRIM(t.govpolicytrngdate), '')::date, 
                                        NULLIF(TRIM(t.inductiontrngdate), '')::date, 
                                        NULLIF(TRIM(t.lastworkingdate), '')::date,
                                        t.licenseno, 
                                        t.licensetype, 
                                        NULLIF(TRIM(t.licenseissuedate), '')::date, 
                                        NULLIF(TRIM(t.licenseexpirydate), '')::date,
                                        t.licensestatus, 
                                        t.orgid,
                                        m.bankacctype,
                                        m.channel,
                                        m.subchannel,
                                        m.agent_type_cat,
                                        m.agent_class,
                                        m.marital_status,
                                        m.education,
                                        m.state,
                                        m.country,
                                        m.gender,
                                        m.title,
                                        m.occupation,
                                        m.agent_sub_type_code,
                                        m.designation_code,
                                        m.agent_type_code,
                                        m.location_code,
                                        m.candidatetype,
                                        m.agenttype,
                                        m.commissionclass
                                    FROM hms.tempagentdto t

                                    LEFT JOIN hms.agent a ON t.Supervisor_Code = a.agent_code AND t.orgid = a.orgid
                                    LEFT JOIN master_map m ON m.agentcode = t.agentcode AND m.orgid = t.orgid

                                    WHERE t.comments = 'Processed'
                                    RETURNING agent_id,agent_code, supervisor_id, created_by, designation_code, orgid
                                ),
                                
                                hierarchy_ins AS (
                                    INSERT INTO hms.agent_hierarchy (agent_id, effective_from_date, designation_code,created_by, created_date, rowversion, hierarchy_path)
                                    SELECT
                                        i.agent_id,
                                        CURRENT_DATE,
                                        i.designation_code,
                                        COALESCE(i.created_by, ''),
                                        NOW(),
                                        1,
                                        CASE
                                            WHEN i.supervisor_id IS NULL
                                                THEN i.agent_id::text::ltree
                                            ELSE (h.hierarchy_path::text || '.' || i.agent_id::text)::ltree
                                        END
                                    FROM ins i
                                    LEFT JOIN hms.agent_hierarchy h
                                        ON i.supervisor_id = h.agent_id AND i.orgid = h.orgid
                                ),
                                
                                nominee_ins AS (
                                    INSERT INTO hms.""Nominee""
                                    (
                                        ""RefKey"",
                                        ""RefType"",
                                        ""NomineeName"",
                                        ""Relationship"",
                                        ""PercentageShare"",
                                        ""NomineeAge"",
                                        ""IsActive"",
                                        ""orgId""
                                    )
                                    SELECT
                                        i.agent_id,
                                        1,
                                        t.nomineename,
                                        t.relationship,
                                        NULLIF(t.percentageshare, '')::numeric(5,2),
                                        NULLIF(t.nomineeage, '')::bigint,
                                        TRUE,
                                        t.orgid
                                    FROM ins i
                                    JOIN hms.tempagentdto t
                                        ON t.agentcode = i.agent_code 
                                       AND t.orgid = i.orgid
                                    LEFT JOIN hms.""Nominee"" n
                                        ON n.""RefKey"" = i.agent_id
                                       AND n.""RefType"" = 1
                                       AND n.""orgId"" = i.orgid
                                    WHERE
                                        t.nomineename IS NOT NULL
                                        AND TRIM(t.nomineename) <> ''
                                        AND n.""NomineeID"" IS NULL
                                ),
                                bank_ins AS (
                                    INSERT INTO hms.""BankAccount""
                                    (
                                        ""RefKey"",
                                        ""RefType"",
                                        ""AccountHolderName"",
                                        ""AccountNumber"",
                                        ""IFSC"",
                                        ""MICR"",
                                        ""BankName"",
                                        ""BranchName"",
                                        ""AccountType"",
                                        ""ActiveSince"",
                                        ""FactoringHouse"",
                                        ""PreferredPaymentMode"",
                                        ""orgId""
                                    )
                                    SELECT
                                        i.agent_id,                      
                                        1,                            
                                        t.accountholdername,
                                        t.accountnumber,
                                        t.ifsc,
                                        t.micr,
                                        t.bankname,
                                        t.Accbranchname,
                                        COALESCE(NULLIF(t.accounttype, '')::int, 1),
                                        COALESCE(NULLIF(TRIM(t.activesince), '')::timestamp, CURRENT_TIMESTAMP),
                                        t.factoringhouse,
                                        COALESCE(NULLIF(t.preferredpaymentmode, '')::int, 1),
                                        t.orgid
                                    FROM ins i
                                    JOIN hms.tempagentdto t
                                        ON t.agentcode = i.agent_code
                                       AND t.orgid = i.orgid
                                    LEFT JOIN hms.""BankAccount"" b
                                        ON b.""RefKey""  = i.agent_id
                                       AND b.""RefType"" = 1
                                       AND b.""orgId""   = i.orgid
                                    WHERE
                                        t.accountnumber IS NOT NULL
                                        AND TRIM(t.accountnumber) <> ''
                                        AND t.ifsc IS NOT NULL
                                        AND TRIM(t.ifsc) <> ''
                                        AND b.""Id"" IS NULL   
                                ),
                                personalinfo_ins AS (
                                        INSERT INTO hms.""PersonalInfo""
                                        (""RefKey"",""RefType"",""DateOfBirth"",""PanNumber"",""Email"",""MobileNo"", ""WorkContactNo"",""ResidenceContactNo"",""BloodGroup"",""BirthPlace"",""MartialStatus"",""EducationCode"",""EducationLevel"",""WorkProfile"",""AnnualIncome"",""WorkExpMonths"",""orgId"")
                                        SELECT
                                            i.agent_id,
                                            1,
                                            NULLIF(t.dateofbirth,'')::date,
                                            t.pannumber,
                                            t.email,
                                            t.mobileno,
                                            t.workcontactno,
                                            t.residencecontactno,
                                            t.bloodgroup,
                                            t.birthplace,
                                            m.marital_status,
                                            m.education,
                                            t.educationlevel,
                                            t.workprofile,
                                            NULLIF(t.annualincome,'')::numeric(18,2),
                                            NULLIF(t.workexpmonths,'')::int,
                                            t.orgid
                                        FROM ins i
                                        JOIN hms.tempagentdto t
                                            ON t.agentcode = i.agent_code
                                           AND t.orgid = i.orgid
                                    
                                        LEFT JOIN hms.""PersonalInfo"" p
                                            ON p.""RefKey"" = i.agent_id
                                           AND p.""RefType"" = 1
                                           AND p.""orgId"" = i.orgid
                                    
                                        LEFT JOIN LATERAL (
                                            SELECT
                                                MAX(CASE WHEN entrycategory = 'MARITAL_STATUS'
                                                         THEN entryidentity END) AS marital_status,
                                                MAX(CASE WHEN entrycategory = 'EDUCATION_CODE'
                                                         THEN entryidentity END) AS education
                                            FROM hmsmaster.keyvalueentries k
                                            WHERE k.orgid = t.orgid
                                              AND (
                                                    (k.entrycategory = 'MARITAL_STATUS' AND k.entrydesc = t.martial_status_desc)
                                                 OR (k.entrycategory = 'EDUCATION_CODE' AND k.entrydesc = t.pinfo_education_desc)
                                              )
                                        ) m ON TRUE
                                    
                                        WHERE
                                            p.""PersonalInfoId"" IS NULL
                                            AND (
                                                t.email IS NOT NULL
                                             OR t.mobileno IS NOT NULL
                                             OR t.dateofbirth IS NOT NULL
                                            )
                                    ),
                                  address_ins AS (
                                            INSERT INTO hms.""Address"" (
                                                ""RefKey"",""RefType"",""AddressType"",
                                                ""AddressLine1"",""AddressLine2"",""AddressLine3"",
                                                ""City"",""State"",""Country"",""PIN"",""Landmark"",""orgId""
                                            )
                                            SELECT DISTINCT ON (i.agent_id)
                                                i.agent_id,
                                                1,
                                                3,
                                                t.addressline1,
                                                t.addressline2,
                                                t.addressline3,
                                                t.city,
                                                m.state_desc,
                                                m.country_desc,
                                                t.pin,
                                                t.landmark,
                                                t.orgid
                                            FROM ins i
                                            JOIN hms.tempagentdto t
                                                ON t.agentcode = i.agent_code
                                               AND t.orgid = i.orgid
                                        
                                            LEFT JOIN hms.""Address"" a
                                                ON a.""RefKey"" = i.agent_id
                                               AND a.""RefType"" = 1
                                               AND a.""AddressType"" = 3
                                        
                                            LEFT JOIN LATERAL (
                                                SELECT
                                                    MAX(CASE WHEN entrycategory = 'STATE_NAME' THEN entrydesc  END) AS state_desc,
                                                    MAX(CASE WHEN entrycategory = 'COUNTRY'    THEN entrydesc  END) AS country_desc
                                                FROM hmsmaster.keyvalueentries k
                                                WHERE k.orgid = t.orgid
                                                  AND (
                                                       (k.entrycategory = 'STATE_NAME' AND k.entrydesc = t.state_desc)
                                                    OR (k.entrycategory = 'COUNTRY'    AND k.entrydesc = t.country_desc)
                                                  )
                                            ) m ON TRUE
                                        
                                            WHERE a.""AddressID"" IS NULL
                                              AND (t.addressline1 IS NOT NULL OR t.city IS NOT NULL OR t.pin IS NOT NULL)
                                        
                                            ORDER BY i.agent_id
                                        )
                                                                
                                SELECT 1;
                        ";

            using var tx = await conn.BeginTransactionAsync();
            using var cmd = new NpgsqlCommand(insertSql, conn, tx);
            cmd.CommandTimeout = 0;
            int affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
    }
}