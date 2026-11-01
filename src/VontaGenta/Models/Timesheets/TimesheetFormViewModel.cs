#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;

namespace VontaGenta.Models
{
    public class TimesheetFormViewModel : IHasProjectsSelectLists
    {
        // For filter on index page
        public DateTime? FilterByDate { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Start date")]
        public DateTime DateStart { get; set; }
        [Required]
        [Display(Name = "Start time")]
        public DateTime TimeStart { get; set; }

        [Display(Name = "End date")]
        public DateTime? DateEnd { get; set; }
        [Display(Name = "End time")]
        public DateTime? TimeEnd { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        public List<Project> ProjectList { get; set; }
        public SelectList Projects { get; set; }

        public DateTime DateTimeStart
        {
            get
            {
                return new DateTime(DateStart.Year, DateStart.Month, DateStart.Day, TimeStart.Hour, TimeStart.Minute, 0);
            }

            set
            {
                DateStart = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
                TimeStart = DateStart;
            }
        }

        public DateTime? DateTimeEnd
        {
            get
            {
                if (!DateEnd.HasValue || !TimeEnd.HasValue)
                {
                    return null;
                }

                return new DateTime(DateEnd.Value.Year, DateEnd.Value.Month, DateEnd.Value.Day, TimeEnd.Value.Hour, TimeEnd.Value.Minute, 0);
            }

            set
            {
                if (value == null)
                {
                    DateEnd = null;
                    TimeEnd = null;
                }
                else
                {
                    DateEnd = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, value.Value.Hour, value.Value.Minute, 0);
                    TimeEnd = DateEnd;
                }
            }
        }
    }
}
