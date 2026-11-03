#nullable disable

namespace VontaGenta.Models
{
    public class CostEntryIndexViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public CostEntryIndexViewModel()
        {
            Year = DateTime.Today.Year;
            Month = DateTime.Today.Month;
        }
    }
}
