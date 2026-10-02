#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VontaGenta.Models
{
    public class CostEntryListViewModel
    {
        // For the field labels
        public CostEntryDetailsViewModel CostEntry { get; set; }
        public List<CostEntryDetailsViewModel> List { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public SelectList MonthsSelectList { get; set; }

        // Pagination
        public int Page { get; set; }
        [Display(Name = "Items per page")]
        public int ItemsPerPage { get; set; }
        public int TotalCount { get; set; }
        // If any items left over add another page to display them.
        public int TotalPages { get { return (TotalCount / ItemsPerPage) + (TotalCount % ItemsPerPage == 0 ? 0 : 1); } }

        public CostEntryListViewModel()
        {
            List = new List<CostEntryDetailsViewModel>();
            Year = DateTime.Today.Year;
            Month = DateTime.Today.Month;

            Page = 1;
            ItemsPerPage = 10;
        }
    }
}
