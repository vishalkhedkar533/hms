using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DB
{
    [Table("keyvalueentries", Schema = "hmsmaster")]
    public class KeyValueEntry
    {
        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("entrycategory")]
        public string EntryCategory { get; set; }

        [Column("entryidentity")]
        public int EntryIdentity { get; set; }

        [Column("entrydesc")]
        public string EntryDesc { get; set; }

        [Column("entryparentid")]
        public int? EntryParentId { get; set; }

        [Column("activestatus")]
        public bool? ActiveStatus { get; set; }
    }
}
