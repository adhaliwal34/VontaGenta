using NodaTime;

namespace VontaGenta.Services
{
    public class AppTimeZonesHelper : IAppTimeZonesHelper
    {

        public DateTime ConvertFromUtcToTimeZone(DateTime utcDateTime, string timeZone)
        {
            var zone = DateTimeZoneProviders.Tzdb[timeZone];
            return Instant.FromDateTimeUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc))
                          .InZone(zone)
                          .ToDateTimeUnspecified();
        }

        public DateTime ConvertFromTimeZoneToUtc(DateTime TimeZoneDateTime, string timeZone)
        {
            var local = LocalDateTime.FromDateTime(TimeZoneDateTime);
            var zone = DateTimeZoneProviders.Tzdb[timeZone];
            ZonedDateTime zonedTime = local.InZoneLeniently(zone);
            return DateTime.SpecifyKind(zonedTime.ToDateTimeUtc(), DateTimeKind.Unspecified); // I don't want .NET to add Z letters to the DateTimes when saving to the database so DateTimeKind must be unspecified.
        }
    }
}
