
namespace VontaGenta.Models
{
    public interface IHasDateCreatedAndUpdated
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
