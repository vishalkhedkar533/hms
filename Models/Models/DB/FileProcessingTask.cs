using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("fileprocessingtasks", Schema = "hms")]
    public class FileProcessingTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("filepath")]
        public string FilePath { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(100)]
        [Column("createdby")]    // <-- lowercase to match table
        public string CreatedBy { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("filename")]
        public string FileName { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("fileextension")]
        public string FileExtension { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("filetype")]
        public string FileType { get; set; }

        [Column("filesize")]
        public long? FileSize { get; set; }

        [Column("isreadonly")]
        public bool? IsReadOnly { get; set; }

        [Column("filecreationtime")]
        public DateTime? FileCreationTime { get; set; }

        [Column("filelastwritetime")]
        public DateTime? FileLastWriteTime { get; set; }

        [Column("totalrows")]
        public int? TotalRows { get; set; }

        [Column("rowsprocessed")]
        public int? RowsProcessed { get; set; }

        [Column("rowsrejected")]
        public int? RowsRejected { get; set; }

        [Required]
        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("startedat")]
        public DateTime? StartedAt { get; set; }

        [Column("completedat")]
        public DateTime? CompletedAt { get; set; }

        [Column("errormessage")]
        public string ErrorMessage { get; set; }

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("successdata")]
        public string? SuccessData { get; set; }
    }
}
