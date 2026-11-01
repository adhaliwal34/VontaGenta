#nullable disable
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class UserOrg
    {
        public int ID { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }
        public int OrgID { get; set; }
        [JsonIgnore]
        public virtual Org Org { get; set; }
        public bool IsActive { get; set; }
    }
}
