#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetCostEntriesDTO : RequestStatus
    {
        public List<CostEntry> List { get; set; }

        public GetCostEntriesDTO()
        {
            List = new List<CostEntry>();
        }
    }

    public class GetTotalCountCostEntryDTO : RequestStatus
    {
        public int Total { get; set; }
    }

    public class GetCostEntryDTO : RequestStatus
    {
        public CostEntry CostEntry { get; set; }
    }

    public class SaveCostEntryDTO : RequestStatus
    {
        public int SavedID { get; set; }
    }

    public class UpdateCostEntryDTO : RequestStatus { }

    public class DeleteCostEntryDTO : RequestStatus { }

}
