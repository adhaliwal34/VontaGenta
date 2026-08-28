#nullable disable
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class AppUser : IdentityUser
    {
        [PersonalData]
        public string FirstNames { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        public int TimeZoneId { get; set; }
        [JsonIgnore]
        public virtual AppTimeZone TimeZone { get; set; }
        [JsonIgnore]
        public string Name { get { return FirstNames != null && LastName != null ? FirstNames + " " + LastName : ""; } }
    }
}
