#nullable disable
using VontaGenta.Models;
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class Timesheet : IHasDateTimeStartAndEnd
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime? DateTimeEnd { get; set; }
        public int ProjectID { get; set; }
        [JsonIgnore]
        public virtual Project Project { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }

        public TimeSpan? GetDuration()
        {
            if (DateTimeEnd == null)
            {
                return null;
            }

            return DateTimeEnd.Value.Subtract(DateTimeStart);
        }

        public Errors Validate()
        {
            var errors = new Errors();

            if (DateTimeEnd.HasValue && DateTimeStart > DateTimeEnd)
            {
                errors.AddError("DateTimeEnd", "End time cannot be less than the start time.");
            }

            if (DateTimeStart == default(DateTime))
            {
                errors.AddError("DateTimeStart", "Start time must be set.");
            }

            if (string.IsNullOrEmpty(Name))
            {
                errors.AddError("Name", "A name must be set.");
            }

            if (ProjectID == default(int))
            {
                errors.AddError("ProjectID", "A project must be set.");
            }

            if (string.IsNullOrEmpty(AppUserID))
            {
                errors.AddError("AppUserID", "An owner must be set.");
            }

            return errors;
        }
    }
}
