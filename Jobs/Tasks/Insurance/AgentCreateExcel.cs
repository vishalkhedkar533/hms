using CommonLibrary;
using Microsoft.Extensions.Configuration;
using MiniExcelLibs;
using Npgsql;
using Repository;
using System.Configuration;
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
        private readonly IConnectionScope _connectionScope; 
        public AgentCreateExcel(IConnectionScope connectionScope)
        {
            _connectionScope = connectionScope;
            _fileService = new FileService(AppContext.BaseDirectory);
            //_connectionString =
            //    "server=ep-silent-silence-a1fanpxl-pooler.ap-southeast-1.aws.neon.tech;" +
            //    "username=neondb_owner;password=npg_MPXYuy4jTe1r;database=neondb;";

            var connectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"];
            _connectionString = connectionStringSettings?.ConnectionString
                ?? throw new Exception("Connection string 'DefaultConnection' not found.");
        }
        public async Task ProcessAgentCreateData()
        {
            var body = _fileService.GetTemplate(Path.Combine("InputStructures"),"excelStructureValidator.json");

            var validatorConfig = JsonSerializer.Deserialize<InputExcelValidator>(body,new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var conn = await _connectionScope.GetOpenConnectionAsync(_connectionString);
            await conn.OpenAsync();

            var fileTasks = await GetPendingTasksAsync((NpgsqlConnection)conn);

            if (!fileTasks.Any())
            {
                Console.WriteLine("No pending file imports found.");
                return;
            }

            int chunkSize = await GetChunkSizeAsync((NpgsqlConnection)conn);

            foreach (var task in fileTasks)
            {
                await ProcessSingleFileAsync(task, (NpgsqlConnection)conn, validatorConfig, chunkSize);
            }

            //await FinalizeDataTransfer(conn);
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
                tasks.Add(new FileProcessingTask
                {
                    FilePath = Path.Combine(AppContext.BaseDirectory, "FileProcess", "TempFile.xlsx"),
                    OrgId = reader.GetInt32(1)
                });
            }

            return tasks;
        }
        private static async Task<int> GetChunkSizeAsync(NpgsqlConnection conn)
        {
            using var cmd = new NpgsqlCommand(
                "SELECT config_value FROM hms.api_config WHERE config_key = 'agent_create_chunk_size' LIMIT 1",
                conn);

            var result = await cmd.ExecuteScalarAsync();
            return int.Parse(result.ToString());
        }
        private async Task ProcessSingleFileAsync(FileProcessingTask task, NpgsqlConnection conn,InputExcelValidator validatorConfig,int chunkSize)
        {
            Console.WriteLine($"Processing file: {task.FilePath}");

            var masterData = await GetMasterDataAsync(conn, task.OrgId ?? 0, "AgentProfileMst");

            var agentClassDict = BuildLookup(masterData, "AGENT_CLASS");
            // keep all your dictionaries as-is...

            var stream = MiniExcel.Query<AgentDto>(task.FilePath);
            List<AgentDto> buffer = new(chunkSize);

            int rowCount = 0;
            int batchNo = 1;

            foreach (var row in stream)
            {
                rowCount++;
                row.OrgId = task.OrgId;

                List<string> errors = new();

                ValidateMaster(row.AgentClassDesc, agentClassDict, v => row.AgentClass = v, "Agent Class", errors);

                if (ValidateRow(row, validatorConfig.excelColumns, rowCount, out var validationErrors))
                    errors.AddRange(validationErrors);

                row.Comments = errors.Any() ? "Rejected" : "Processed";
                row.Reason = string.Join(" | ", errors);

                buffer.Add(row);

                if (buffer.Count == chunkSize)
                {
                    await BulkCopy(buffer, conn, batchNo++);
                    buffer.Clear();
                }
            }

            if (buffer.Any())
                await BulkCopy(buffer, conn, batchNo);

            Console.WriteLine($"Completed {rowCount} rows.");
        }

        private static bool ValidateRow(AgentDto row, Excelcolumn[] validationRules, int rowNumber, out List<string> errors)
        {
            errors = new List<string>();
            bool isValid = true;
            Type dtoType = typeof(AgentDto);

            foreach (var rule in validationRules)
            {
                var propertyInfo = dtoType.GetProperty(rule.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (propertyInfo == null)
                {
                    errors.Add($"Row {rowNumber}: Config column '{rule.ColumnName}' not found in AgentDto.");
                    isValid = false;
                    continue;
                }

                var propertyValue = propertyInfo.GetValue(row)?.ToString();
                var originalValue = propertyValue;

                if (rule.TrimContent && propertyValue != null)
                {
                    propertyValue = propertyValue.Trim();
                }

                // Required Check
                if (!rule.AllowBlank && string.IsNullOrWhiteSpace(propertyValue))
                {
                    errors.Add($"Row {rowNumber}, Column '{rule.ColumnName}': **Required** field cannot be blank.");
                    isValid = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(propertyValue)) continue;

                // RegEx / Data Format Validation
                if (rule.UseRegEx)
                {
                    try
                    {
                        if (!Regex.IsMatch(propertyValue, rule.DataFormat))
                        {
                            errors.Add($"Row {rowNumber}, Column '{rule.ColumnName}' (Value: '{originalValue}'): Does not match required Regular Expression format.");
                            isValid = false;
                        }
                    }
                    catch (ArgumentException)
                    {
                        errors.Add($"Row {rowNumber}, Column '{rule.ColumnName}': Internal Error - Malformed Regex Pattern in config.");
                        isValid = false;
                    }
                }
                else if (rule.DestinationDataType.Contains("DateTime"))
                {
                    if (!DateTime.TryParseExact(propertyValue, rule.DataFormat,
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None, out _))
                    {
                        errors.Add($"Row {rowNumber}, Column '{rule.ColumnName}' (Value: '{originalValue}'): Invalid date format. Expected: {rule.DataFormat}.");
                        isValid = false;
                    }
                }
            }
            return isValid;
        }

        private static string Escape(object value)
        {
            if (value == null) return "";
            return "\"" + value.ToString().Replace("\"", "\"\"") + "\"";
        }

        private static async Task BulkCopy(List<AgentDto> rows, NpgsqlConnection conn, int batchNo)
        {
            Console.WriteLine($"Processing Batch {batchNo} ({rows.Count} rows)...");

            string copySql = @"
        COPY hms.tempagentdto
        (
            AgentId, AgentCode,AgentName,
            BusinessName, FirstName, MiddleName, LastName, Prefix,
            Suffix, DOB, Nationality,
            PreferredLanguage,
            AgentLevel, StaffCode, Supervisor_Code, ContractedDate,
            AgentStatusCode, StatusDate, IsLicensed, MaskedPanNumber, aadhaar_number,
            IrdaLicenseNumber, GstNumber, CreatedBy, CreatedDate, ModifiedBy,
            ModifiedDate, RowVersion, IsActive, ApplicationDocketNo,
            Father_Husband_Nm, EmployeeCode,
            StartDate, PanAadharLinkFlag, Sec206abFlag, PackageID,
            TaxStatus, StateEid, URN,
            AdditionalComment, AppointmentDate, IncorporationDate, CnctPersonDesig, CnctPersonMobileNo,
            CnctPersonEmail, CnctPersonName, CMSAgentType,
            ServiceTaxNo, UlipFlag, TrainingGroupType, Ifs, RefresherTrainingCompleted,
            IsMigrated, MainPartnerClientCode, AgentMaincodevwEid, RegistrationDate, Vertical,
            BranchCode, BranchName, Ic36TrngCompletionDate, STrngCompletionDate, ConfirmationDate,
            FgRockstarTrainingDate, IncrementDate, LastPromotionDate, HRDoj, FgValueTrngDate,
            HSecPolicyTrngDate, ItSecPolicyTrngDate, NpsTrngCompletionDate, WhistleBlowerTrngDate, GovPolicyTrngDate,
            InductionTrngDate, LastWorkingDate, LicenseNo, LicenseType, LicenseIssueDate,
            LicenseExpiryDate, LicenseStatus,AddressLine1, AddressLine2, AddressLine3, City, PIN, Landmark,Comments, Reason,orgid,
            agent_class_desc, bank_acc_type_desc, gender_desc, title_desc, channel_desc, sub_channel_desc, occupation_desc, agent_type_cat_desc, 
            marital_status_desc, education_desc, state_desc, country_desc, designation_code_desc, location_code_desc, agent_type_code_desc, 
            agent_sub_type_code_desc, candidate_type_desc, commission_class_desc, agent_type_desc,
            NomineeName, Relationship, PercentageShare, NomineeAge,
            accountholdername, accountnumber, ifsc, micr, bankname, Accbranchname, accounttype, activesince, factoringhouse,preferredpaymentmode,
            dateofbirth,pannumber,email,mobileno,workcontactno,residencecontactno,bloodgroup,birthplace,martial_status_desc,pinfo_education_desc,educationlevel,workprofile,annualincome,workexpmonths
        )
        FROM STDIN WITH (FORMAT CSV);
    ";

            using var writer = conn.BeginTextImport(copySql);

            foreach (var r in rows)
            {
                await writer.WriteLineAsync(string.Join(",", new[]
                {
            Escape(r.AgentId),
            Escape(r.AgentCode),
            Escape(r.AgentName),
            Escape(r.BusinessName),
            Escape(r.FirstName),
            Escape(r.MiddleName),
            Escape(r.LastName),
            Escape(r.Prefix),
            Escape(r.Suffix),
            Escape(r.DOB),
            Escape(r.Nationality),
            Escape(r.PreferredLanguage),
            Escape(r.AgentLevel),
            Escape(r.StaffCode),
            Escape(r.Supervisor_Code),
            Escape(r.ContractedDate),
            Escape(r.AgentStatusCode),
            Escape(r.StatusDate),
            Escape(r.IsLicensed),
            Escape(r.MaskedPanNumber),
            Escape(r.aadhaar_number),
            Escape(r.IrdaLicenseNumber),
            Escape(r.GstNumber),
            Escape(r.CreatedBy),
            Escape(r.CreatedDate),
            Escape(r.ModifiedBy),
            Escape(r.ModifiedDate),
            Escape(r.RowVersion),
            Escape(r.IsActive),
            Escape(r.ApplicationDocketNo),
            Escape(r.Father_Husband_Nm),
            Escape(r.EmployeeCode),
            Escape(r.StartDate),
            Escape(r.PanAadharLinkFlag),
            Escape(r.Sec206abFlag),
            Escape(r.PackageID),
            Escape(r.TaxStatus),
            Escape(r.StateEid),
            Escape(r.URN),
            Escape(r.AdditionalComment),
            Escape(r.AppointmentDate),
            Escape(r.IncorporationDate),
            Escape(r.CnctPersonDesig),
            Escape(r.CnctPersonMobileNo),
            Escape(r.CnctPersonEmail),
            Escape(r.CnctPersonName),
            Escape(r.CMSAgentType),
            Escape(r.ServiceTaxNo),
            Escape(r.UlipFlag),
            Escape(r.TrainingGroupType),
            Escape(r.Ifs),
            Escape(r.RefresherTrainingCompleted),
            Escape(r.IsMigrated),
            Escape(r.MainPartnerClientCode),
            Escape(r.AgentMaincodevwEid),
            Escape(r.RegistrationDate),
            Escape(r.Vertical),
            Escape(r.BranchCode),
            Escape(r.BranchName),
            Escape(r.Ic36TrngCompletionDate),
            Escape(r.STrngCompletionDate),
            Escape(r.ConfirmationDate),
            Escape(r.FgRockstarTrainingDate),
            Escape(r.IncrementDate),
            Escape(r.LastPromotionDate),
            Escape(r.HRDoj),
            Escape(r.FgValueTrngDate),
            Escape(r.HSecPolicyTrngDate),
            Escape(r.ItSecPolicyTrngDate),
            Escape(r.NpsTrngCompletionDate),
            Escape(r.WhistleBlowerTrngDate),
            Escape(r.GovPolicyTrngDate),
            Escape(r.InductionTrngDate),
            Escape(r.LastWorkingDate),
            Escape(r.LicenseNo),
            Escape(r.LicenseType),
            Escape(r.LicenseIssueDate),
            Escape(r.LicenseExpiryDate),
            Escape(r.LicenseStatus),
            Escape(r.AddressLine1),
            Escape(r.AddressLine2),
            Escape(r.AddressLine3),
            Escape(r.City),
            Escape(r.Pin),
            Escape(r.Landmark),
            Escape(r.Comments),
            Escape(r.Reason),
            Escape(r.OrgId),
            Escape(r.AgentClassDesc),
            Escape(r.BankAccTypeDesc),
            Escape(r.GenderDesc),
            Escape(r.TitleDesc),
            Escape(r.ChannelDesc),
            Escape(r.SubChannelDesc),
            Escape(r.OccupationDesc),
            Escape(r.AgentTypeCatDesc),
            Escape(r.MaritalStatusDesc),
            Escape(r.EducationDesc),
            Escape(r.StateDesc),
            Escape(r.CountryDesc),
            Escape(r.DesignationCodeDesc),
            Escape(r.LocationCodeDesc),
            Escape(r.AgentTypeCodeDesc),
            Escape(r.AgentSubTypeCodeDesc),
            Escape(r.CandidateTypeDesc),
            Escape(r.CommissionClassDesc),
            Escape(r.AgentTypeDesc),
            Escape(r.NomineeName),
            Escape(r.Relationship),
            Escape(r.PercentageShare),
            Escape(r.NomineeAge),
            Escape(r.AccountHolderName),
            Escape(r.AccountNumber),
            Escape(r.IFSC),
            Escape(r.MICR),
            Escape(r.BankName),
            Escape(r.BankAccBranchName),
            Escape(r.AccountType),
            Escape(r.ActiveSince),
            Escape(r.FactoringHouse),
            Escape(r.PreferredPaymentMode),
            Escape(r.DateOfBirth),
            Escape(r.PanNumber),
            Escape(r.Email),
            Escape(r.MobileNo),
            Escape(r.WorkContactNo),
            Escape(r.ResidenceContactNo),
            Escape(r.BloodGroup),
            Escape(r.BirthPlace),
            Escape(r.MartialStatus),
            Escape(r.EducationCode),
            Escape(r.EducationLevel),
            Escape(r.WorkProfile),
            Escape(r.AnnualIncome),
            Escape(r.WorkExpMonths)
        }));
            }

            await writer.DisposeAsync();
            Console.WriteLine($"Batch {batchNo} inserted.");
        }
        static async Task<List<KeyValueEntry>> GetMasterDataAsync(NpgsqlConnection conn, int orgId, string masterName)
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

        static Dictionary<string, int> BuildLookup(List<KeyValueEntry> list, string category)
        {
            return list
                .Where(x => x.EntryCategory == category && !string.IsNullOrWhiteSpace(x.EntryDesc))
                .ToDictionary(
                    x => x.EntryDesc.Trim().ToLower(),
                    x => x.EntryIdentity
                );
        }

        static void ValidateMaster(string? descValue, Dictionary<string, int> lookup, Action<int> assign, string fieldName, List<string> errors)
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
        private static async Task FinalizeDataTransfer(NpgsqlConnection conn)
        {
            Console.WriteLine("\n--- Finalizing Data Transfer (set-based) ---");

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
                                    t.agentcode, t.agentname,
                                    t.businessname, t.firstname, t.middlename, t.lastname, t.prefix,
                                    t.suffix, t.dob::date, t.nationality,
                                    t.preferredlanguage,
                                    CASE TRIM(t.agentlevel)
                                      WHEN 'Level1' THEN 1 WHEN 'Level2' THEN 2 WHEN 'Level3' THEN 3 ELSE NULL END,
                                    t.staffcode, t.contracteddate::date, t.agentstatuscode, t.statusdate::date,
                                    CASE WHEN t.islicensed IN ('Y','y','1') THEN TRUE WHEN t.islicensed IN ('N','n','0') THEN FALSE ELSE FALSE END,
                                    t.maskedpannumber, t.aadhaar_number, t.irdalicensenumber, t.gstnumber,
                                    t.createdby, t.createddate::date, t.modifiedby, t.modifieddate::date,
                                    NULLIF(TRIM(t.rowversion), '')::integer,
                                    a.agent_id::integer,
                                    CASE WHEN t.isactive IN ('Y','y','1') THEN TRUE WHEN t.isactive IN ('N','n','0') THEN FALSE ELSE FALSE END,
                                    t.applicationdocketno, t.father_husband_nm,
                                    t.employeecode, t.startdate::date,
                                    CASE WHEN t.panaadharlinkflag IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                                    CASE WHEN t.sec206abflag IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                                    t.taxstatus, t.urn,
                                    t.additionalcomment, t.appointmentdate::date, t.incorporationdate::date,
                                    t.cnctpersondesig, t.cnctpersonmobileno, t.cnctpersonemail, t.cnctpersonname,
                                    t.cmsagenttype, t.packageid, t.servicetaxno,
                                    CASE WHEN t.ulipflag IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                                    t.traininggrouptype, t.ifs,
                                    CASE WHEN t.refreshertrainingcompleted IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                                    CASE WHEN t.ismigrated IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                                    t.mainpartnerclientcode, t.agentmaincodevweid, t.registrationdate::date,
                                    t.vertical, t.branchcode, t.branchname, t.ic36trngcompletiondate::date,
                                    t.strngcompletiondate::date, t.confirmationdate::date,
                                    t.fgrockstartrainingdate::date, t.incrementdate::date, t.lastpromotiondate::date,
                                    t.hrdoj::date, t.fgvaluetrngdate::date, t.hsecpolicytrngdate::date,
                                    t.itsecpolicytrngdate::date, t.npstrngcompletiondate::date, t.whistleblowertrngdate::date,
                                    t.govpolicytrngdate::date, t.inductiontrngdate::date, t.lastworkingdate::date,
                                    t.licenseno, t.licensetype, t.licenseissuedate::date, t.licenseexpirydate::date,
                                    t.licensestatus, t.orgid,
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
                                        COALESCE(NULLIF(t.activesince, '')::timestamp, CURRENT_TIMESTAMP),
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
        //public void ProcessAgentCreateData()
        //{
        //}
    }
}
