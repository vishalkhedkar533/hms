namespace Tasks.Models
{
    public class KeyValueEntry
    {
        public int orgid { get; set; }
        public string EntryCategory { get; set; }
        public int EntryIdentity { get; set; }
        public string EntryDesc { get; set; }
        public int? EntryParentId { get; set; }
        public bool? ActiveStatus { get; set; }
    }
}
