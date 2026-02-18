using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("ui_components", Schema = "hmsmaster")]
    public class UiComponent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("component_id")]
        public int ComponentId { get; set; }

        /// <summary>
        /// PostgreSQL ltree type represented as a string.
        /// Format: 'Top.Child.Grandchild'
        /// </summary>
        [Required]
        [Column("path")]
        public string Path { get; set; } = string.Empty;

        [Required]
        [Column("label")]
        public string Label { get; set; } = string.Empty;

        [Required]
        [Column("elementtype")]
        public string ElementType { get; set; } = string.Empty;

        // Navigation Property: One Component can have many Fields
        public virtual ICollection<UiField> Fields { get; set; } = new List<UiField>();
    }
}
