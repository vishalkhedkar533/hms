using Models.Enums;

namespace Models.DTO
{
    public class SearchMenu
    {
        public int? RoleId { get; set; }
        public MenuSearchFor SearchFor { get; set; }
    }
}