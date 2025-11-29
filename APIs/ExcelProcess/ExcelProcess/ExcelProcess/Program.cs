using CommonLibrary;
using MiniExcelLibs;
using Models.DTO;
using Npgsql;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

const string TemplateRootPath = @"E:\HMS\Code\APIs\HMS\HMS\";
var _fileService = new FileService(TemplateRootPath);
var Body= _fileService.GetTemplate(Path.Combine("InputStructures"), "excelStructureValidator.json");

var validatorConfig = JsonSerializer.Deserialize<InputExcelValidator>(Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

string connectionString =
    "server=ep-silent-silence-a1fanpxl-pooler.ap-southeast-1.aws.neon.tech;" +
    "username=neondb_owner;password=npg_MPXYuy4jTe1r;database=neondb;";

Console.WriteLine("Enter Excel file path:");
string filePath = Console.ReadLine();

if (!File.Exists(filePath))
{
    Console.WriteLine("File not found!");
    return;
}

using var conn = new NpgsqlConnection(connectionString);
conn.Open();

int chunkSize = 0;
using (var cmd = new NpgsqlCommand(
    "SELECT config_value FROM hms.api_config WHERE config_key = 'agent_create_chunk_size' LIMIT 1",
    conn))
{
    var result = await cmd.ExecuteScalarAsync();
    chunkSize = int.Parse(result.ToString());
}
int batchNo = 1;
var stream = MiniExcel.Query<AgentDto>(filePath);

List<AgentDto> buffer = new(chunkSize);
List<string> errorLog = new();
int rowCount = 0;

foreach (var row in stream)
{
    rowCount++;

    List<string> errors;

    // Run Validation
    if (ValidateRow(row, validatorConfig.excelColumns, rowCount, out errors))
    {
        // VALID ROW: Mark as Processed and clear errors
        row.Comments = "Processed";
        row.Reason = string.Empty;
    }
    else
    {
        // INVALID ROW: Mark as Rejected and set Reason
        row.Comments = "Rejected";
        // Combine all errors into the Reason column
        row.Reason = string.Join(" | ", errors);
    }

    // Add ALL rows (valid or invalid) to the buffer for staging
    buffer.Add(row);

    if (buffer.Count == chunkSize)
    {
        // All rows in the buffer are copied to hms.tempagentdto
        await BulkCopy(buffer, conn, batchNo);
        buffer.Clear();
        batchNo++;
    }
}

// Process remaining batch
if (buffer.Count > 0)
{
    await BulkCopy(buffer, conn, batchNo);
}

await FinalizeDataTransfer(conn);


Console.WriteLine("\nBULK INSERT COMPLETE.");
Console.WriteLine($"Total rows processed from Excel: {rowCount}.");

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
            Escape(r.Supervisor_Id),
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
            Escape(r.Reason)
        }));
    }

    string copySql = @"
        COPY hms.tempagentdto
        (
            AgentId, AgentCode, AgentTypeCode, AgentSubTypeCode, AgentName,
            BusinessName, FirstName, MiddleName, LastName, Prefix,
            Suffix, Gender, DOB, Nationality, MaritalStatusCode,
            PreferredLanguage, ChannelCode, SubChannelCode, DesignationCode, Designation,
            AgentLevel, LocationCode, StaffCode, Supervisor_Id, ContractedDate,
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
            LicenseExpiryDate, LicenseStatus,AddressLine1, AddressLine2, AddressLine3, City, State, Country, PIN, Landmark,Comments, Reason
        )
        FROM STDIN WITH (FORMAT CSV);
    ";

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
    Console.WriteLine("\n--- Finalizing Data Transfer and Cleaning Staging Table ---");

    string sql = @"
    INSERT INTO hms.agent (
        agent_code, agent_type_code, agent_sub_type_code, agent_name,
        business_name, first_name, middle_name, last_name, prefix,
        suffix, gender, dob, nationality, marital_status_code,
        preferred_language, channel_code, sub_channel_code, designation_code, agent_level,
        location_code, staff_code, contracted_date, agent_status_code, status_date,
        is_licensed, pan_number, aadhaar_number, irda_license_number, gst_number,
        created_by, created_date, modified_by, modified_date, rowversion,
        supervisor_id, is_active, candidatetype, -- Removed email and mobileno here
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
        licenseissuedate, licenseexpirydate, licensestatus
    )

    SELECT
    agentcode, 
    agenttypecode, 
    agentsubtypecode, 
    agentname,
    businessname, 
    firstname, 
    middlename, 
    lastname, 
    prefix,
    suffix, 
    gender, 
    dob::date, -- Date fix
    nationality, 
    maritalstatuscode,
    preferredlanguage, 
    channelcode, 
    subchannelcode, 
    designationcode, 
    CASE TRIM(agentlevel)
    WHEN 'Level1' THEN 1
    WHEN 'Level2' THEN 2
    WHEN 'Level3' THEN 3
    WHEN '' THEN NULL
    ELSE NULL 
    END AS agentlevel,
    locationcode, 
    staffcode, 
    contracteddate::date, -- Date fix
    agentstatuscode, 
    statusdate::date, -- Date fix (Assumed, based on pattern)
    CASE 
        WHEN islicensed IN ('Y', 'y', '1') THEN TRUE 
        WHEN islicensed IN ('N', 'n', '0') THEN FALSE
        ELSE FALSE -- Use FALSE instead of NULL to satisfy the NOT NULL constraint
    END AS islicensed,
    maskedpannumber, 
    aadhaar_number, 
    irdalicensenumber, 
    gstnumber,
    createdby, 
    createddate::date, -- Date fix (Assumed)
    modifiedby, 
    modifieddate::date, -- Date fix (Assumed)
    NULLIF(TRIM(rowversion), '')::integer AS rowversion, -- Integer fix
    NULLIF(TRIM(supervisor_id), '')::integer AS supervisor_id, -- Integer fix
    CASE 
    WHEN isactive IN ('Y', 'y', '1') THEN TRUE 
    WHEN isactive IN ('N', 'n', '0') THEN FALSE
    ELSE FALSE -- Changed from NULL to FALSE
    END AS isactive,
    candidatetype,
    applicationdocketno, 
    title, 
    father_husband_nm, 
    channel_name, 
    sub_channel,
    employeecode, 
    startdate::date, -- Date fix (Assumed)
    CASE 
        WHEN panaadharlinkflag IN ('Y', 'y', '1', 'TRUE', 'T') THEN TRUE 
        WHEN panaadharlinkflag IN ('N', 'n', '0', 'FALSE', 'F') THEN FALSE
        ELSE FALSE
    END AS panaadharlinkflag, 
    CASE 
        WHEN sec206abflag IN ('Y', 'y', '1', 'TRUE', 'T') THEN TRUE 
        WHEN sec206abflag IN ('N', 'n', '0', 'FALSE', 'F') THEN FALSE
        ELSE FALSE
    END AS sec206abflag, 
    commissionclass,
    taxstatus, 
    stateeid, 
    occupationcode::integer, 
    occupation, 
    urn,
    additionalcomment, 
    appointmentdate::date, -- Date fix (Assumed)
    incorporationdate::date, -- Date fix (Assumed)
    cnctpersondesig, 
    cnctpersonmobileno,
    cnctpersonemail, 
    cnctpersonname, 
    agenttypecategory, 
    agentclassification, 
    cmsagenttype,
    packageid, 
    servicetaxno,
    CASE 
        WHEN ulipflag IN ('Y', 'y', '1', 'TRUE', 'T') THEN TRUE 
        WHEN ulipflag IN ('N', 'n', '0', 'FALSE', 'F') THEN FALSE
        ELSE FALSE
    END AS ulipflag,
    traininggrouptype, 
    ifs,
    CASE 
        WHEN refreshertrainingcompleted IN ('Y', 'y', '1', 'TRUE', 'T') THEN TRUE 
        WHEN refreshertrainingcompleted IN ('N', 'n', '0', 'FALSE', 'F') THEN FALSE
        ELSE FALSE
    END AS refreshertrainingcompleted,
    CASE 
        WHEN ismigrated IN ('Y', 'y', '1', 'TRUE', 'T') THEN TRUE 
        WHEN ismigrated IN ('N', 'n', '0', 'FALSE', 'F') THEN FALSE
        ELSE FALSE
    END AS ismigrated, 
    mainpartnerclientcode, 
    agentmaincodevweid, 
    registrationdate::date, -- Date fix (Assumed)
    vertical, 
    branchcode, 
    branchname, 
    ic36trngcompletiondate::date, -- Date fix (Assumed)
    strngcompletiondate::date, -- Date fix (Assumed)
    confirmationdate::date, -- Date fix (Assumed)
    fgrockstartrainingdate::date, -- Date fix (Assumed)
    incrementdate::date, -- Date fix (Assumed)
    lastpromotiondate::date, -- Date fix (Assumed)
    hrdoj::date, -- Date fix (Assumed)
    fgvaluetrngdate::date, -- Date fix (Assumed)
    hsecpolicytrngdate::date, -- Date fix (Assumed)
    itsecpolicytrngdate::date, -- Date fix (Assumed)
    npstrngcompletiondate::date, -- Date fix (Assumed)
    whistleblowertrngdate::date, -- Date fix (Assumed)
    govpolicytrngdate::date, -- Date fix (Assumed)
    inductiontrngdate::date, -- Date fix (Assumed)
    lastworkingdate::date, -- Date fix (Assumed)
    licenseno, 
    licensetype,
    licenseissuedate::date, -- Date fix (Assumed)
    licenseexpirydate::date, -- Date fix (Assumed)
    licensestatus
FROM
    hms.tempagentdto
WHERE
    ""comments"" = 'Processed';
    ";

    using var cmd = new NpgsqlCommand(sql, conn);
    int rowsMoved = await cmd.ExecuteNonQueryAsync();

    Console.WriteLine($"{rowsMoved} valid rows successfully moved to hms.agent.");
}

//--2.Optional: CLEAN UP the staging table (if necessary, though sometimes you keep rejected rows for audit)
//        -- DELETE FROM hms.tempagentdto WHERE Comments = 'Processed';


//string sql = @"
//    INSERT INTO hms.agent (/* ... all columns ... */)
//    SELECT
//        t.agentcode, /* ... all 82 columns ... */
//    FROM
//        hms.tempagentdto AS t -- Use alias 't' for the staging table
//    WHERE
//        t.""comments"" = 'Processed'
//        -- 🛑 FIX: Skip any rows where the agentcode already exists in the target table
//        AND NOT EXISTS (
//            SELECT 1
//            FROM hms.agent AS a
//            WHERE a.agent_code = t.agentcode
//        );
//    ";


//string sql = @"
//    INSERT INTO hms.agent (/* ... all columns ... */)
//    SELECT
//        /* ... all 82 columns from t ... */
//    FROM
//        hms.tempagentdto AS t
//    WHERE
//        t.""comments"" = 'Processed'

//    ON CONFLICT (agent_code) -- Key column
//    DO UPDATE SET
//        agent_name = EXCLUDED.agent_name,
//        agent_level = EXCLUDED.agent_level,
//        status_date = EXCLUDED.status_date,
//        modified_date = NOW(); -- Add all fields that should be updated
//    ";
