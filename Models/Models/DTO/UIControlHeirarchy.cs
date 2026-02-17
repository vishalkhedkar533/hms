using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class UIMenuHeirarchyDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("menu_id")]
        public long MenuId { get; set; }

        [JsonPropertyName("allow_Edit")]
        public bool AllowEdit { get; set; }

        [JsonPropertyName("allow_Read")]
        public bool AllowRead { get; set; }

        [JsonPropertyName("render_control")]
        public bool RenderControl { get; set; }

        [JsonPropertyName("access_granted_by")]
        public int? AccessGrantedBy { get; set; }

        [JsonPropertyName("access_granted_on")]
        public DateTime? AccessGrantedOn { get; set; }

        [JsonPropertyName("children")]
        public List<UIMenuHeirarchyDTO> Children { get; set; } = new();
    }
}
