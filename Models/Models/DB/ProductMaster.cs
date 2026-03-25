using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DB
{
    [Table("product_master", Schema = "hmsmaster")]
    public class ProductMaster
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required, StringLength(10)]
        [Column("product_code")]
        public string ProductCode { get; set; } = null!;

        [Required, StringLength(100)]
        [Column("product_name")]
        public string ProductName { get; set; } = null!;

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("effective_from")]
        public DateTime EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime EffectiveTo { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("modified_by")]
        public int? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }
    }

    public class ProductSaveDto
    {
        public int? ProductId { get; set; } 
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int CategoryId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool? IsActive { get; set; }
    }
}
