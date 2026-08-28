#nullable disable

namespace VontaGenta.Models
{
    public class RevenueIndexViewModel : BaseViewModel
    {
        // Used for displaying the property names in the view only
        public RevenueDetailsViewModel Revenue { get; set; }
        public List<RevenueDetailsViewModel> Revenues { get; set; }

        public DateTime? FilterByDate { get; set; }

        public DateTime? PreviousDay { get { return FilterByDate?.AddDays(-1); } }
        public DateTime Today { get; set; }
        public DateTime? NextDay { get { return FilterByDate?.AddDays(1); } }

        public RevenueIndexViewModel()
        {
            Revenues = new List<RevenueDetailsViewModel>();
        }
    }
}
