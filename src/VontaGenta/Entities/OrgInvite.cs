#nullable disable
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class OrgInvite
    {
        public int ID { get; set; }
        public string ToUserID { get; set; }
        [JsonIgnore]
        public virtual AppUser ToUser { get; set; }
        public int OrgID { get; set; }
        [JsonIgnore]
        public virtual Org Org { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var orgInvite = obj as OrgInvite;
            if (orgInvite == null)
            {
                return false;
            }

            return orgInvite.ToUserID == ToUserID && orgInvite.OrgID == OrgID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ToUserID, OrgID);
        }
    }
}
