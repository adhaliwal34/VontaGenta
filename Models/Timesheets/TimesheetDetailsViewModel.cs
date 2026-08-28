#nullable disable

using VontaGenta.Entities;
using System.ComponentModel.DataAnnotations;

namespace VontaGenta.Models
{
    public class TimesheetDetailsViewModel
    {
        public DateTime? FilterByDate { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Start date")]
        public DateTime DateStart { get; set; }
        [Display(Name = "Start time")]
        public DateTime TimeStart { get; set; }

        [Display(Name = "End date")]
        public DateTime? DateEnd { get; set; }
        [Display(Name = "End time")]
        public DateTime? TimeEnd { get; set; }
        [Display(Name = "Duration (hh:mm)")]
        public string Duration { get; set; }
        [Display(Name = "Project")]
        public string ProjectName { get; set; }
    }
}
