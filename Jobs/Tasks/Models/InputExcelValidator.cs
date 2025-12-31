namespace Tasks.Models
{
    public class InputExcelValidator
    {
        public int OrgId { get; set; }
        public string InputType { get; set; }
        public string[] fileExtenstion { get; set; }
        public Excelcolumn[] excelColumns { get; set; }
    }

    public class Excelcolumn
    {
        public string ColumnName { get; set; }
        public string DestinationDataType { get; set; }
        public string DataFormat { get; set; }
        public bool UseRegEx { get; set; }
        public bool AllowBlank { get; set; }
        public bool TrimContent { get; set; }
    }
}
