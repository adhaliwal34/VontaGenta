#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public class RevenueCreateViewModel : IHasProjectsSelectLists, IHasCurrenciesSelectLists
    {
        // For filter on index page
        public DateTime? FilterByDate { get; set; }
        public int ID { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
        [Range(0.0, 9999999.99, ErrorMessage = "Amount must be between 0.0 and 9,999,999.99")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public int CurrencyID { get; set; }
        public List<Currency> CurrencyList { get; set; }
        public SelectList Currencies { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime EntryDate { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        public List<Project> ProjectList { get; set; }
        public SelectList Projects { get; set; }

    }
}
