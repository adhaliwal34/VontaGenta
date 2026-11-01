#nullable disable
using VontaGenta.AppEnums;

namespace VontaGenta.Entities
{
    public class DataLog
    {
        public int ID { get; set; }
        public string EntityName { get; set; }
        public int EntityID { get; set; }
        public AppActions Action { get; set; }
        public DateTime EntryTimestamp { get; set; }
        public string AppUserID { get; set; }
        public virtual AppUser AppUser { get; set; }
        public string Json { get; set; }
    }
}
