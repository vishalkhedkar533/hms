namespace Models.DTO
{

    public class UIMenuResponse
    {
        public List<UIComponentDto> UIMenu { get; set; }
    }

    public class UIComponentDto
    {
        public string Type { get; set; }
        public string Section { get; set; }
        public int? componentId { get; set; }
        public string? componentName { get; set; }
        public List<UIComponentDto>? SubSection { get; set; }
        public List<Fieldlist>? FieldList { get; set; }
    }


    public class Fieldlist
    {
        public bool render { get; set; }
        public int cntrlid { get; set; }
        public bool allowedit { get; set; }
        public string cntrlName { get; set; }
        public int? access_granted_by { get; set; }
        public DateTime? access_granted_on { get; set; }
        public int? approverOneRoleId { get; set; }
        public int? approverTwoRoleId { get; set; }
        public int? approverThreeRoleId { get; set; }
        public bool useDefaultApprover { get; set; } = false;
    }


    //public class UIMenuResponse
    //{
    //    [JsonPropertyName("UIMenu")]
    //    public List<UIComponentDto> UIMenu { get; set; } = new();
    //}

    //public class UIComponentDto
    //{
    //    [JsonPropertyName("Section")]
    //    public string Section { get; set; } = string.Empty;

    //    [JsonPropertyName("Type")]
    //    public string Type { get; set; } = string.Empty;

    //    // This property allows for infinite nesting (Screen -> Tab -> Section, etc.)
    //    [JsonPropertyName("SubSection")]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public List<UIComponentDto>? SubSection { get; set; }

    //    [JsonPropertyName("FieldList")]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public List<FieldDto>? FieldList { get; set; }
    //}

    //public class FieldDto
    //{
    //    [JsonPropertyName("cntrlid")]
    //    public int CntrlId { get; set; }

    //    [JsonPropertyName("cntrlName")]
    //    public string CntrlName { get; set; } = string.Empty;

    //    [JsonPropertyName("render")]
    //    public bool Render { get; set; }

    //    [JsonPropertyName("allowedit")]
    //    public bool AllowEdit { get; set; }
    //}
}
