using CommonLibrary;
using MiniExcelLibs;
using Models.DB;
using Models.DTO;
using Npgsql;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var _fileService = new FileService(AppContext.BaseDirectory);
var Body = _fileService.GetTemplate(Path.Combine("InputStructures"), "excelStructureValidator.json");

var validatorConfig = JsonSerializer.Deserialize<InputExcelValidator>(Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

string connectionString =
    "server=ep-silent-silence-a1fanpxl-pooler.ap-southeast-1.aws.neon.tech;" +
    "username=neondb_owner;password=npg_MPXYuy4jTe1r;database=neondb;";

using var conn = new NpgsqlConnection(connectionString);
conn.Open();

Console.WriteLine("Fetching pending file-import tasks...");

List<FileProcessingTask> fileTasks = new();

using (var cmd = new NpgsqlCommand(
    @"SELECT filepath, orgid 
      FROM hms.fileprocessingtasks 
      WHERE status = 'Pending';", conn))
{
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        fileTasks.Add(new FileProcessingTask
        {
            FilePath = Path.Combine(AppContext.BaseDirectory, "FileProcess", "TempFile.xlsx"), //reader.GetString(0),
            OrgId = reader.GetInt32(1)
        });
    }
}

if (!fileTasks.Any())
{
    Console.WriteLine("No pending file imports found.");
    return;
}

Console.WriteLine($"Found {fileTasks.Count} file(s) to process.\n");

int chunkSize = 0;
using (var cmd = new NpgsqlCommand(
    "SELECT config_value FROM hms.api_config WHERE config_key = 'agent_create_chunk_size' LIMIT 1",
    conn))
{
    var result = await cmd.ExecuteScalarAsync();
    chunkSize = int.Parse(result.ToString());
}
foreach (var task in fileTasks)
{
    Console.WriteLine($"Processing file: {task.FilePath}, Org: {task.OrgId}");

    var stream = MiniExcel.Query<AgentDto>(task.FilePath);
    List<AgentDto> buffer = new(chunkSize);

    int rowCount = 0;
    int batchNo = 1;

    foreach (var row in stream)
    {
        rowCount++;

        // Attach OrgId
        row.OrgId = (int)task.OrgId;

        List<string> errors;

        // VALIDATION
        if (ValidateRow(row, validatorConfig.excelColumns, rowCount, out errors))
        {
            row.Comments = "Processed";
            row.Reason = string.Empty;
        }
        else
        {
            row.Comments = "Rejected";
            row.Reason = string.Join(" | ", errors);
        }

        buffer.Add(row);

        if (buffer.Count == chunkSize)
        {
            await BulkCopy(buffer, conn, batchNo);
            buffer.Clear();
            batchNo++;
        }
    }

    // LAST REMAINING ROWS
    if (buffer.Count > 0)
        await BulkCopy(buffer, conn, batchNo);

    Console.WriteLine($"Completed: {task.FilePath} — {rowCount} rows processed.\n");
}

await FinalizeDataTransfer(conn);
static async Task BulkCopy(List<AgentDto> rows, NpgsqlConnection conn, int batchNo)
{
    Console.WriteLine($"Processing Batch {batchNo} ({rows.Count} rows)...");

    var sb = new StringBuilder();

    foreach (var r in rows)
    {
        sb.AppendLine(string.Join(",", new string[]
        {
            Escape(r.AgentId),
            Escape(r.AgentCode),
            Escape(r.AgentTypeCode),
            Escape(r.AgentSubTypeCode),
            Escape(r.AgentName),
            Escape(r.BusinessName),
            Escape(r.FirstName),
            Escape(r.MiddleName),
            Escape(r.LastName),
            Escape(r.Prefix),
            Escape(r.Suffix),
            Escape(r.Gender),
            Escape(r.DOB),
            Escape(r.Nationality),
            Escape(r.MaritalStatusCode),
            Escape(r.PreferredLanguage),
            Escape(r.ChannelCode),
            Escape(r.SubChannelCode),
            Escape(r.DesignationCode),
            Escape(r.Designation),
            Escape(r.AgentLevel),
            Escape(r.LocationCode),
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
            Escape(r.CandidateType),
            Escape(r.ApplicationDocketNo),
            Escape(r.Title),
            Escape(r.Father_Husband_Nm),
            Escape(r.Channel_Name),
            Escape(r.Sub_Channel),
            Escape(r.EmployeeCode),
            Escape(r.StartDate),
            Escape(r.PanAadharLinkFlag),
            Escape(r.Sec206abFlag),
            Escape(r.PackageID),
            Escape(r.CommissionClass),
            Escape(r.TaxStatus),
            Escape(r.StateEid),
            Escape(r.OccupationCode),
            Escape(r.Occupation),
            Escape(r.URN),
            Escape(r.AdditionalComment),
            Escape(r.AppointmentDate),
            Escape(r.IncorporationDate),
            Escape(r.CnctPersonDesig),
            Escape(r.CnctPersonMobileNo),
            Escape(r.CnctPersonEmail),
            Escape(r.CnctPersonName),
            Escape(r.AgentTypeCategory),
            Escape(r.AgentClassification),
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
            Escape(r.State),
            Escape(r.Country),
            Escape(r.Pin),
            Escape(r.Landmark),
            Escape(r.Comments),
            Escape(r.Reason),
            Escape(r.OrgId)
        }));
    }

    string copySql = @"
        COPY hms.tempagentdto
        (
            AgentId, AgentCode, AgentTypeCode, AgentSubTypeCode, AgentName,
            BusinessName, FirstName, MiddleName, LastName, Prefix,
            Suffix, Gender, DOB, Nationality, MaritalStatusCode,
            PreferredLanguage, ChannelCode, SubChannelCode, DesignationCode, Designation,
            AgentLevel, LocationCode, StaffCode, Supervisor_Code, ContractedDate,
            AgentStatusCode, StatusDate, IsLicensed, MaskedPanNumber, aadhaar_number,
            IrdaLicenseNumber, GstNumber, CreatedBy, CreatedDate, ModifiedBy,
            ModifiedDate, RowVersion, IsActive, CandidateType, ApplicationDocketNo,
            Title, Father_Husband_Nm, Channel_Name, Sub_Channel, EmployeeCode,
            StartDate, PanAadharLinkFlag, Sec206abFlag, PackageID, CommissionClass,
            TaxStatus, StateEid, OccupationCode, Occupation, URN,
            AdditionalComment, AppointmentDate, IncorporationDate, CnctPersonDesig, CnctPersonMobileNo,
            CnctPersonEmail, CnctPersonName, AgentTypeCategory, AgentClassification, CMSAgentType,
            ServiceTaxNo, UlipFlag, TrainingGroupType, Ifs, RefresherTrainingCompleted,
            IsMigrated, MainPartnerClientCode, AgentMaincodevwEid, RegistrationDate, Vertical,
            BranchCode, BranchName, Ic36TrngCompletionDate, STrngCompletionDate, ConfirmationDate,
            FgRockstarTrainingDate, IncrementDate, LastPromotionDate, HRDoj, FgValueTrngDate,
            HSecPolicyTrngDate, ItSecPolicyTrngDate, NpsTrngCompletionDate, WhistleBlowerTrngDate, GovPolicyTrngDate,
            InductionTrngDate, LastWorkingDate, LicenseNo, LicenseType, LicenseIssueDate,
            LicenseExpiryDate, LicenseStatus,AddressLine1, AddressLine2, AddressLine3, City, State, Country, PIN, Landmark,Comments, Reason,orgid
        )
        FROM STDIN WITH (FORMAT CSV);
    ";

    /* Step 1
     * Excel will contain
     * public string? AgentClassDesc { get; set; }
        public string? BankAccTypeDesc { get; set; }
        public string? GenderDesc { get; set; }
        public string? TitleDesc { get; set; }
        public string? ChannelDesc { get; set; }
        public string? SubChannelDesc { get; set; }
        public string? OccupationDesc { get; set; }
        public string? AgentTypeCatDesc { get; set; }
        public string? MaritalStatusDesc { get; set; }
        public string? EducationDesc { get; set; }
        public string? StateDesc { get; set; }
        public string? CountryDesc { get; set; }
        public string? DesignationCodeDesc { get; set; }
        public string? LocationCodeDesc { get; set; }
        public string? AgentTypeCodeDesc { get; set; }
        public string? AgentSubTypeCodeDesc { get; set; }
        public string? CandidateTypeDesc { get; set; }
        public string? CommissionClassDesc { get; set; }
        public string? AgentTypeDesc { get; set; }
     */

    /* Step 2
     * Get master from var AgentProfileMst = GetMasterData("AgentProfileMst").ToList();
     */

    /*
     * Check if the values exist in master data, if yes, then map the description to code.
     * if not, mark the records as rejected with reason "Invalid <FieldName>"
     * update hms.tempagentdto 
     set "comments" = "comments" + 'Error in BankAcc Details'
     where not exists (
     select 1 
     from hmsmaster.keyvalueentries 
     where hms.tempagentdto.BankAccTypeDesc = hmsmaster.keyvalueentries.entrydesc 
      and hms.tempagentdto.orgid = hms.tempagentdto.orgid)
     */

    using var writer = conn.BeginTextImport(copySql);

    await writer.WriteAsync(sb.ToString());
    await writer.DisposeAsync();

    Console.WriteLine($"Batch {batchNo} inserted.");
}

static string Escape(object value)
{
    if (value == null) return "";
    return "\"" + value.ToString().Replace("\"", "\"\"") + "\"";
}

static bool ValidateRow(AgentDto row, Excelcolumn[] validationRules, int rowNumber, out List<string> errors)
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
        //else if (rule.DestinationDataType.Contains("Boolean"))
        //{
        //    if (!bool.TryParse(propertyValue, out _))
        //    {
        //        errors.Add($"Row {rowNumber}, Column '{rule.ColumnName}' (Value: '{originalValue}'): Invalid boolean format. Expected: True or False.");
        //        isValid = false;
        //    }
        //}
    }
    return isValid;
}

static async Task FinalizeDataTransfer(NpgsqlConnection conn)
{
    Console.WriteLine("\n--- Finalizing Data Transfer (set-based) ---");

    string insertSql = @"WITH ins AS (
                          INSERT INTO hms.agent (
                            -- same columns as before...
                            agent_code, agent_type_code, agent_sub_type_code, agent_name,
                            business_name, first_name, middle_name, last_name, prefix,
                            suffix, gender, dob, nationality, marital_status_code,
                            preferred_language, channel_code, sub_channel_code, designation_code, agent_level,
                            location_code, staff_code, contracted_date, agent_status_code, status_date,
                            is_licensed, pan_number, aadhaar_number, irda_license_number, gst_number,
                            created_by, created_date, modified_by, modified_date, rowversion,
                            supervisor_id, is_active, candidatetype,
                            applicationdocketno, title, father_husband_nm, channel_name, sub_channel,
                            employeecode, startdate, panaadharlinkflag, sec206abflag, commissionclass,
                            taxstatus, stateeid, occupationcode, occupation, urn,
                            additionalcomment, appointmentdate, incorporationdate, cnctpersondesig, cnctpersonmobileno,
                            cnctpersonemail, cnctpersonname, agenttypecategory, agentclassification, cmsagenttype,
                            packageid, servicetaxno, ulipflag, traininggrouptype, ifs,
                            refreshertrainingcompleted, ismigrated, mainpartnerclientcode, agentmaincodevweid, registrationdate,
                            vertical, branchcode, branchname, ic36trngcompletiondate, strngcompletiondate,
                            confirmationdate, fgrockstartrainingdate, incrementdate, lastpromotiondate, hrdoj,
                            fgvaluetrngdate, hsecpolicytrngdate, itsecpolicytrngdate, npstrngcompletiondate, whistleblowertrngdate,
                            govpolicytrngdate, inductiontrngdate, lastworkingdate, licenseno, licensetype,
                            licenseissuedate, licenseexpirydate, licensestatus, orgid
                          )
                          SELECT
                            t.agentcode, t.agenttypecode, t.agentsubtypecode, t.agentname,
                            t.businessname, t.firstname, t.middlename, t.lastname, t.prefix,
                            t.suffix, t.gender, t.dob::date, t.nationality, t.maritalstatuscode,
                            t.preferredlanguage, t.channelcode, t.subchannelcode, t.designationcode,
                            CASE TRIM(t.agentlevel)
                              WHEN 'Level1' THEN 1 WHEN 'Level2' THEN 2 WHEN 'Level3' THEN 3 ELSE NULL END,
                            t.locationcode, t.staffcode, t.contracteddate::date, t.agentstatuscode, t.statusdate::date,
                            CASE WHEN t.islicensed IN ('Y','y','1') THEN TRUE WHEN t.islicensed IN ('N','n','0') THEN FALSE ELSE FALSE END,
                            t.maskedpannumber, t.aadhaar_number, t.irdalicensenumber, t.gstnumber,
                            t.createdby, t.createddate::date, t.modifiedby, t.modifieddate::date,
                            NULLIF(TRIM(t.rowversion), '')::integer,
                            a.agent_id::integer,
                            CASE WHEN t.isactive IN ('Y','y','1') THEN TRUE WHEN t.isactive IN ('N','n','0') THEN FALSE ELSE FALSE END,
                            t.candidatetype, t.applicationdocketno, t.title, t.father_husband_nm, t.channel_name, t.sub_channel,
                            t.employeecode, t.startdate::date,
                            CASE WHEN t.panaadharlinkflag IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                            CASE WHEN t.sec206abflag IN ('Y','y','1','T','TRUE') THEN TRUE ELSE FALSE END,
                            t.commissionclass, t.taxstatus, t.stateeid, t.occupationcode::integer, t.occupation, t.urn,
                            t.additionalcomment, t.appointmentdate::date, t.incorporationdate::date,
                            t.cnctpersondesig, t.cnctpersonmobileno, t.cnctpersonemail, t.cnctpersonname,
                            t.agenttypecategory, t.agentclassification, t.cmsagenttype, t.packageid, t.servicetaxno,
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
                            t.licensestatus, t.orgid
                          FROM hms.tempagentdto t left join hms.agent a on t.Supervisor_Code = a.agent_code and t.orgid = a.orgid
                          WHERE t.comments = 'Processed'
                          RETURNING agent_id, supervisor_id, created_by, channel_code, designation_code, orgid
                        )
                        INSERT INTO hms.agent_hierarchy (agent_id, effective_from_date, channel_code, designation_code, created_by, created_date, rowversion, hierarchy_path)
                        SELECT
                          i.agent_id,
                          CURRENT_DATE,
                          i.channel_code,
                          i.designation_code,
                          COALESCE(i.created_by, '')::text,
                          NOW(),
                          1,
                          CASE WHEN i.supervisor_id IS NULL THEN i.agent_id::text::ltree
                               ELSE (h.hierarchy_path ::text || '.' || i.agent_id::text)::ltree
                          END
                        FROM ins i left join hms.agent_hierarchy h on i.supervisor_id = h.agent_id and i.orgid = h.orgid;";

    using var tx = await conn.BeginTransactionAsync();
    using var cmd = new NpgsqlCommand(insertSql, conn, tx);
    cmd.CommandTimeout = 0; // or a large value (seconds) so it doesn't time out
    int affected = await cmd.ExecuteNonQueryAsync();
    await tx.CommitAsync();

    Console.WriteLine("Agents + hierarchy inserted (set-based).");
}