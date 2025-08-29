using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DB
{
    [Table("channeldetails", Schema = "hms")] // Quoted "channeldetails" handled via EF Table attribute
    public class ChannelDetails
    {
        [Key]
        [Column("ChannelUserId")]
        public int UserId { get; set; }
      
        [Column("ChannelId")]
        public long? ChannelId { get; set; }
        [Column("ChannelName")]
        public string? ChannelName { get; set; }
        [Column("TotalEntities")]
        public long? TotalEntities { get; set; } = 0;
        [Column("Created")]
        public long? Created { get; set; } = 0;
        [Column("Terminated")]
        public long? Terminated { get; set; } = 0;

    }
}
