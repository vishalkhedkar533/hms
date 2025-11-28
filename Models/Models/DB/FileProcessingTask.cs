using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("FileProcessingTasks", Schema = "hms")]
    public class FileProcessingTask
    {
        // 1. Id (SERIAL NOT NULL PRIMARY KEY)
        [Key] // Designates this property as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indicates the database generates the value (like SERIAL)
        public int Id { get; set; }
        // 2. FilePath (VARCHAR(500) NOT NULL)
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }
        // 3. Status (VARCHAR(20) NOT NULL DEFAULT 'Pending')
        [Required]
        [MaxLength(20)]
        // Note: The default value 'Pending' is usually handled in the database or during object creation in C#
        public string Status { get; set; } = "Pending";
        // 4. CreatedBy (VARCHAR(100) NOT NULL)
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; }
        // --- FileInfo Properties ---
        // 5. FileName (VARCHAR(255) NOT NULL)
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }
        // 6. FileExtension (VARCHAR(50) NOT NULL)
        [Required]
        [MaxLength(50)]
        public string FileExtension { get; set; }
        // 7. FileSize (BIGINT NULL)
        public long? FileSize { get; set; } // Use 'long?' (nullable long) for BIGINT NULL
        // 8. IsReadOnly (BOOLEAN NULL)
        public bool? IsReadOnly { get; set; } // Use 'bool?' (nullable boolean)
        // 9. FileCreationTime (TIMESTAMP WITHOUT TIME ZONE NULL)
        public DateTime? FileCreationTime { get; set; } // Use 'DateTime?' for nullable TIMESTAMP
        // 10. FileLastWriteTime (TIMESTAMP WITHOUT TIME ZONE NULL)
        public DateTime? FileLastWriteTime { get; set; }
        // --- Numeric Fields for Tracking Row Counts ---
        // 11. TotalRows (INTEGER NULL)
        public int? TotalRows { get; set; } // Use 'int?' for nullable INTEGER
        // 12. RowsProcessed (INTEGER NULL)
        public int? RowsProcessed { get; set; }
        // 13. RowsRejected (INTEGER NULL)
        public int? RowsRejected { get; set; }
        // --- Timestamp Fields ---
        // 14. CreatedAt (TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW())
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Set default in C# or rely on DB default
        // 15. StartedAt (TIMESTAMP WITHOUT TIME ZONE NULL)
        public DateTime? StartedAt { get; set; }
        // 16. CompletedAt (TIMESTAMP WITHOUT TIME ZONE NULL)
        public DateTime? CompletedAt { get; set; }
        // 17. ErrorMessage (TEXT NULL)
        public string ErrorMessage { get; set; } // string maps to TEXT/VARCHAR(MAX) by default
    }
}