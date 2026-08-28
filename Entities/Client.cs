#nullable disable
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class Client
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }
        public int OrgID { get; set; }
        [JsonIgnore]
        public virtual Org Org { get; set; }
        [JsonIgnore]
        public string ClientIdName { get { return $"{ID}:{Name}"; } }
    }
}
