#nullable disable

namespace VontaGenta.Models
{
    public class TimesheetIndexViewModel : BaseViewModel
    {
        // Used for displaying the property names in the view only
        public TimesheetDetailsViewModel Timesheet { get; set; }
        public List<TimesheetDetailsViewModel> Timesheets { get; set; }

        public DateTime? FilterByDate { get; set; }

        public DateTime? PreviousDay { get { return FilterByDate?.AddDays(-1); } }
        public DateTime Today { get; set; }
        public DateTime? NextDay { get { return FilterByDate?.AddDays(1); } }

        public Dictionary<string, TimeSpan> TotalTimePerProject { get; set; }

        public TimesheetIndexViewModel()
        {
            Timesheets = new List<TimesheetDetailsViewModel>();
            TotalTimePerProject = new Dictionary<string, TimeSpan>();
        }
    }
}
