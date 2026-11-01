
namespace VontaGenta.Models
{
    public interface IHasDateTimeStartAndEnd
    {
        public DateTime DateTimeStart { get; set; }
        public DateTime? DateTimeEnd { get; set; }
    }
}
