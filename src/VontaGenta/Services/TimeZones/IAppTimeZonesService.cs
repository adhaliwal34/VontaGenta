using VontaGenta.Models;

namespace VontaGenta.Services
{
    public interface IAppTimeZonesService
    {
        void ConvertFromTimeZoneToUtc(string timeZone, IHasDateTimeStartAndEnd item);
        void ConvertFromUtcToTimeZone(string timeZone, IHasDateTimeStartAndEnd item);
        DateTime ConvertFromUtcToTimeZone(string timeZone, DateTime utcDateTime);
        void ConvertFromUtcToTimeZone<T>(string timeZone, T item) where T : class, IHasDateTimeStartAndEnd, IHasDateCreatedAndUpdated;
    }
}
