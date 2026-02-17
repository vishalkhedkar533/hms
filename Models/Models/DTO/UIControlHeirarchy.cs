using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class UIMenuHeirarchyDTO
    {
        [JsonPropertyName("menuID")]
        public int MenuID { get; set; }

        [JsonPropertyName("menuName")]
        public string MenuName { get; set; }

        [JsonPropertyName("menuType")]
        public string MenuType { get; set; }

        [JsonPropertyName("allowEdit")]
        public bool? AllowEdit { get; set; }

        [JsonPropertyName("hierarchyId")]
        public int HierarchyId { get; set; }

        [JsonPropertyName("childControl")]
        public UIMenuHeirarchyDTO ChildControl { get; set; }

        [JsonPropertyName("renderControl")]
        public bool? RenderControl { get; set; }

        [JsonPropertyName("accessGrantedBy")]
        public int? AccessGrantedBy { get; set; }

        [JsonPropertyName("accessGrantedOn")]
        public DateTime? AccessGrantedOn { get; set; }
    }
}
