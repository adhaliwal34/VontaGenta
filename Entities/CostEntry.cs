#nullable disable
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class CostEntry
    {
        public int ID { get; set; }
        public DateTime EntryDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        [JsonIgnore]
        public virtual Category Category { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        [JsonIgnore]
        public virtual Currency Currency { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }
        public int ProjectID { get; set; }
        [JsonIgnore]
        public virtual Project Project { get; set; }
    }
}
