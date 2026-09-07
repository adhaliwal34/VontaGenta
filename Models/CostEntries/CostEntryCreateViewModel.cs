#nullable disable
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;
using System.ComponentModel.DataAnnotations;
namespace VontaGenta.Models
{
    public class CostEntryCreateViewModel : IHasCategoriesSelectLists, IHasProjectsSelectLists, IHasCurrenciesSelectLists
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Date")]
        public DateTime EntryDate { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Category> CategoryList { get; set; }
        [Display(Name = "Category")]
        public int CategoryID { get; set; }
        public SelectList Categories { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        [Display(Name = "Currency")]
        public int CurrencyID { get; set; }
        public List<Currency> CurrencyList { get; set; }
        public SelectList Currencies { get; set; }
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        public List<Project> ProjectList { get; set; }
        public SelectList Projects { get; set; }

        // For redirection
        public int? Year { get; set; }
        public int? Month { get; set; }
        // Redirection pagination fields
        public int? Page { get; set; }
        public int? ItemsPerPage { get; set; }

        public CostEntryCreateViewModel()
        {
            EntryDate = DateTime.Today;
            ProjectList = [];
        }
    }
}
