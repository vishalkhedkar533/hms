using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("api_config", Schema = "hms")]
    public class ApiConfig
    {
        [Key]
        [Column("config_key")]
        [StringLength(50)]
        public string ConfigKey { get; set; } = string.Empty;

        [Required]
        [Column("config_value")]
        [StringLength(20)]
        public string ConfigValue { get; set; } = string.Empty;
    }
}
