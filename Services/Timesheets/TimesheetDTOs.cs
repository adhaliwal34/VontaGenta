#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetTimesheetsDTO : RequestStatus
    {
        public DateTime? FromDate { get; set; }
        public Dictionary<string, TimeSpan> TotalTimePerProject { get; set; }
        public List<Timesheet> List { get; set; }
        public Dictionary<int, string> TimesheetDuration { get; set; }

        public GetTimesheetsDTO()
        {
            List = new List<Timesheet>();
            TotalTimePerProject = new Dictionary<string, TimeSpan>();
            TimesheetDuration = new Dictionary<int, string>();
        }
    }

    public class GetTimesheetDTO : RequestStatus
    {
        public Timesheet Timesheet { get; set; }
    }

    public class SaveTimesheetDTO : RequestStatus { }

    public class UpdateTimesheetDTO : RequestStatus { }

    public class DeleteTimesheetDTO : RequestStatus { }

}
