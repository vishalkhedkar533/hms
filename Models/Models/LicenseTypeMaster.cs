using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace HMS.Models
{
    /// <summary>
    /// Master table for license types (e.g., LIFE, GENERAL, COMPOSITE).
    /// </summary>
    [Table("LICENSE_TYPE_MASTER", Schema = "hms")]
    public class LicenseTypeMaster
    {
        [Key]
        [Column("LICENSE_TYPE_CODE")]
        [StringLength(20)]
        [SwaggerSchema("Unique code for the license type (e.g., LIFE, GENERAL).")]
        public string LicenseTypeCode { get; set; } = null!;

        [Required]
        [Column("LICENSE_TYPE_NAME")]
        [StringLength(100)]
        [SwaggerSchema("Descriptive name of the license type.")]
        public string LicenseTypeName { get; set; } = null!;

        [Column("DESCRIPTION")]
        [StringLength(255)]
        [SwaggerSchema("Optional description of the license type.")]
        public string? Description { get; set; }

        [Required]
        [Column("IS_ACTIVE")]
        [SwaggerSchema("Indicates whether the license type is active.")]
        public bool IsActive { get; set; }

        [Required]
        [Column("CREATED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who created the record.")]
        public string CreatedBy { get; set; } = null!;

        [Required]
        [Column("CREATED_DATE")]
        [SwaggerSchema("Date and time when the record was created.")]
        public DateTime CreatedDate { get; set; }

        [Column("MODIFIED_BY")]
        [StringLength(100)]
        [SwaggerSchema("User who last modified the record.")]
        public string? ModifiedBy { get; set; }

        [Column("MODIFIED_DATE")]
        [SwaggerSchema("Date and time when the record was last modified.")]
        public DateTime? ModifiedDate { get; set; }

        [Column("ROWVERSION")]
        [SwaggerSchema("Row version for o]()
