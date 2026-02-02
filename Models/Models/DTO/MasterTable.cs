namespace Models.DTO
{
    public class MasterTable
    {
        /// <summary>
        /// Maps to orgid (int4 - typically int in C#). NOT NULL.
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// Maps to EntryCategory (int). NOT NULL.
        /// </summary>
        public string EntryCategory { get; set; }

        /// <summary>
        /// Maps to SchemaName (varchar(500)). NOT NULL.
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// Maps to TableName (varchar(500)). NOT NULL.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Maps to FilterCriteria (varchar(1000)). NULLable.
        /// </summary>
        public string? FilterCriteria { get; set; } = string.Empty;
        public string? columnalias { get; set; } = string.Empty;
    }
}
