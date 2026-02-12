using SharedModels.BackEndCalculation;

namespace Models.DTO
{
    public class GridDTOHeader
    {
        public Int64 TotalRecords = 0;
        public Int64 TotalPages = 0;
        public Int64 CurrentPage = 0;
    }
    public class GridDTOBody
    {
        public List<Agent> Agents = new List<Agent>();
    }
    public class GridDTO 
    {
        public GridDTOHeader Header = new GridDTOHeader();
        public GridDTOBody Body = new GridDTOBody();
    }
}
