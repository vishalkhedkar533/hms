using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("custom_fields", Schema = "hms")]
    public class CustomField
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("field_id")]
        public int FieldId { get; set; }

        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("label")]
        public string Label { get; set; } = string.Empty;

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("placeholder")]
        public string? Placeholder { get; set; }

        [Column("required")]
        public bool Required { get; set; }

        [Column("min_length")]
        public int? MinLength { get; set; }

        [Column("max_length")]
        public int? MaxLength { get; set; }

        [Column("pattern")]
        public string? Pattern { get; set; }

        [Column("validation_message")]
        public string? ValidationMessage { get; set; }

        [Column("options", TypeName = "text")]
        public string? Options { get; set; }

        [Column("target_screen")]
        public string TargetScreen { get; set; } = string.Empty;

        [Column("target_tab")]
        public string TargetTab { get; set; } = string.Empty;

        [Column("target_section")]
        public string TargetSection { get; set; } = string.Empty;

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("orgid")]
        public int? OrgId { get; set; }
    }
}
