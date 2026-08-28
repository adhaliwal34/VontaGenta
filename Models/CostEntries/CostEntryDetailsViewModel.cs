#nullable disable
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class CostEntryDetailsViewModel
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencySymbol { get; set; }
        [Display(Name = "Creator")]
        public string UserName { get; set; }
        [Display(Name = "Project")]
        public string ProjectName { get; set; }
        // For redirection
        public int? Year { get; set; }
        public int? Month { get; set; }
        // Redirection pagination fields
        public int? Page { get; set; }
        public int? ItemsPerPage { get; set; }
    }
}
