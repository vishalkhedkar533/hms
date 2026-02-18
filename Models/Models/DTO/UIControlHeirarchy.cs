using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class UIMenuResponse
    {
        [JsonPropertyName("UIMenu")]
        public List<UIComponentDto> UIMenu { get; set; } = new();
    }

    public class UIComponentDto
    {
        [JsonPropertyName("Section")]
        public string Section { get; set; } = string.Empty;

        [JsonPropertyName("Type")]
        public string Type { get; set; } = string.Empty;

        // This property allows for infinite nesting (Screen -> Tab -> Section, etc.)
        [JsonPropertyName("SubSection")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<UIComponentDto>? SubSection { get; set; }

        [JsonPropertyName("FieldList")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<FieldDto>? FieldList { get; set; }
    }

    public class FieldDto
    {
        [JsonPropertyName("cntrlid")]
        public int CntrlId { get; set; }

        [JsonPropertyName("cntrlName")]
        public string CntrlName { get; set; } = string.Empty;

        [JsonPropertyName("render")]
        public bool Render { get; set; }

        [JsonPropertyName("allowedit")]
        public bool AllowEdit { get; set; }
    }
}
