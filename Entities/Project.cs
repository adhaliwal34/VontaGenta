#nullable disable

using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class Project
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AppUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser User { get; set; }
        public int ClientID { get; set; }
        [JsonIgnore]
        public virtual Client Client { get; set; }
        [JsonIgnore]
        public string ProjectIdName { get { return $"{ID}:{Name}"; } }
    }
}
