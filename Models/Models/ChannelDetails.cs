using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Models
{
    [Table("channeldetails", Schema = "hms")] // Quoted "channeldetails" handled via EF Table attribute
    public class ChannelDetails
    {
        [Key]
        [Column("ChannelUserId")]
        public int UserId { get; set; }
      
        [Column("ChannelId")]
        public Int64? ChannelId { get; set; }
        [Column("ChannelName")]
        public string? ChannelName { get; set; }
        [Column("TotalEntities")]
        public Int64? TotalEntities { get; set; } = 0;
        [Column("Created")]
        public Int64? Created { get; set; } = 0;
        [Column("Terminated")]
        public Int64? Terminated { get; set; } = 0;

    }
}
