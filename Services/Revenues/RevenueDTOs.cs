#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetRevenuesDTO : RequestStatus
    {
        public DateTime? Date { get; set; }
        public List<Revenue> List { get; set; }

        public GetRevenuesDTO()
        {
            List = new List<Revenue>();
        }
    }

    public class GetRevenueDTO : RequestStatus
    {
        public Revenue Revenue { get; set; }
    }

    public class SaveRevenueDTO : RequestStatus { }

    public class UpdateRevenueDTO : RequestStatus { }

    public class DeleteRevenueDTO : RequestStatus { }

}
