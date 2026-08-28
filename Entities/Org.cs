#nullable disable
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class Org
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }
    }
}
