using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    //[Table("Nominee", Schema = "hms")]
    public class Nominee
    {
        //[Column("NomineeID")]
        public int NomineeID { get; set; }
        //[Column("AgentID")]
        public int AgentID { get; set; }
        //[Column("NomineeName")]
        public string NomineeName { get; set; } = null!;
        //[Column("Relationship")]
        public string Relationship { get; set; } = null!;
        //[Column("PercentageShare")]
        public decimal PercentageShare { get; set; }
        //[Column("IsActive")]
        public bool IsActive { get; set; } = true;
        public Int64 NomineeAge { get; set; }
    }
}
