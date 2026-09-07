#nullable disable
using Newtonsoft.Json;
using VontaGenta.Models;

namespace VontaGenta.Entities
{
    public class Revenue
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        [JsonIgnore]
        public virtual Currency Currency { get; set; }
        public int ProjectID { get; set; }
        [JsonIgnore]
        public virtual Project Project { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }


        public Errors Validate()
        {
            var errors = new Errors();

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
