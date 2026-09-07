#nullable disable

using System.ComponentModel.DataAnnotations;

namespace VontaGenta.Models
{
    public class RevenueDetailsViewModel
    {
        public DateTime? FilterByDate { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CurrencySymbol { get; set; }
        [Display(Name = "Date")]
        public DateTime EntryDate { get; set; }
        
        [Display(Name = "Project")]
        public string ProjectName { get; set; }
    }
}
