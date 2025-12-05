using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniExcelLibs;
using Models.DTO;
using Npgsql;
using System.Text;

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

foreach (var row in stream)
{
    buffer.Add(row);

    if (buffer.Count == chunkSize)
    {
        await BulkCopy(buffer, conn, batchNo);
        buffer.Clear();
        batchNo++;
    }
}

//if (buffer.Count > 0)
//    await BulkCopy(buffer, conn, batchNo);

Console.WriteLine("\nALL DATA INSERTED SUCCESSFULLY!");

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
            Escape(r.Landmark)
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
            LicenseExpiryDate, LicenseStatus,AddressLine1, AddressLine2, AddressLine3, City, State, Country, PIN, Landmark
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
