using VontaGenta.Entities;
using VontaGenta.Models;

namespace VontaGenta.Mapping
{
    public static class TimesheetsMappings
    {
        public static void MapTo(this Timesheet source, TimesheetDetailsViewModel dest)
        {
            dest.ID = source.ID;
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.DateStart = source.DateTimeStart.Date;
            dest.TimeStart = TruncateToMinute(source.DateTimeStart);
            dest.DateEnd = source.DateTimeEnd?.Date;
            dest.TimeEnd = source.DateTimeEnd.HasValue ? TruncateToMinute(source.DateTimeEnd.Value) : null;
            dest.ProjectName = source.Project?.Name;
        }

        public static void MapTo(this Timesheet source, TimesheetFormViewModel dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.DateStart = source.DateTimeStart.Date;
            dest.TimeStart = TruncateToMinute(source.DateTimeStart);
            dest.DateTimeEnd = source.DateTimeEnd;
            dest.ProjectID = source.ProjectID;
        }

        public static void MapTo(this TimesheetFormViewModel source, Timesheet dest)
        {
            dest.Name = source.Name;
            dest.Description = source.Description;
            dest.DateTimeStart = source.DateTimeStart;
            dest.DateTimeEnd = source.DateTimeEnd;
            dest.ProjectID = source.ProjectID;
        }

        private static DateTime TruncateToMinute(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }
    }
}
