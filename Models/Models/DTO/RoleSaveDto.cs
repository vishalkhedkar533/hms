using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class RoleSaveDto
    {
        [SwaggerSchema("Only required for Updates. Leave null for New roles.")]
        public long? Role_ID { get; set; }

        [Required(ErrorMessage = "Role name is mandatory")]
        [StringLength(100)]
        public string RoleName { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsSystemRole { get; set; }

        public bool IsActive { get; set; }

        [SwaggerSchema("Concurrency token for updates")]
        public int? RowVersion { get; set; }
    }
}
