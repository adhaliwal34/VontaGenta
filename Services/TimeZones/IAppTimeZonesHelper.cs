
namespace VontaGenta.Services
{
    public interface IAppTimeZonesHelper
    {
        DateTime ConvertFromTimeZoneToUtc(DateTime timeZoneDateTime, string timeZone);
        DateTime ConvertFromUtcToTimeZone(DateTime utcDateTime, string timeZone);
    }
}