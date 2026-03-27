namespace Models.DTO
{
    public class CustomFieldDto
    {
        public int FieldId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public bool Required { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public string? ValidationMessage { get; set; }
        public string? Options { get; set; }
        public string TargetScreen { get; set; } = string.Empty;
        public string TargetTab { get; set; } = string.Empty;
        public string TargetSection { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveCustomFieldRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Placeholder { get; set; }
        public bool Required { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public string? ValidationMessage { get; set; }
        public string? Options { get; set; }
        public string TargetScreen { get; set; } = string.Empty;
        public string TargetTab { get; set; } = string.Empty;
        public string TargetSection { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FetchCustomFieldsRequest
    {
        public string TargetScreen { get; set; } = string.Empty;
        public string TargetTab { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}
