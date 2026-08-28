using VontaGenta.Models;

namespace VontaGenta.Services
{
    public class AppTimeZonesService : IAppTimeZonesService
    {
        private readonly IAppTimeZonesHelper _appTimeZonesHelper;

        public AppTimeZonesService(IAppTimeZonesHelper appTimeZonesHelper)
        {
            _appTimeZonesHelper = appTimeZonesHelper;
        }

        /*
         * See Various-Date_and_time.md
         */
        public void ConvertFromUtcToTimeZone(string timeZone, IHasDateTimeStartAndEnd item)
        {
            if (item == null)
            {
                return;
            }

            item.DateTimeStart = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateTimeStart, timeZone);

            if (item.DateTimeEnd.HasValue)
            {
                item.DateTimeEnd = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateTimeEnd.Value, timeZone);
            }
        }

        public DateTime ConvertFromUtcToTimeZone(string timeZone, DateTime utcDateTime)
        {
            return _appTimeZonesHelper.ConvertFromUtcToTimeZone(utcDateTime, timeZone);
        }

        public void ConvertFromUtcToTimeZone<T>(string timeZone, T item) where T : class, IHasDateTimeStartAndEnd, IHasDateCreatedAndUpdated
        {
            if (item == null)
            {
                return;
            }

            item.DateTimeStart = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateTimeStart, timeZone);

            if (item.DateTimeEnd.HasValue)
            {
                item.DateTimeEnd = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateTimeEnd.Value, timeZone);
            }

            if (item.DateCreated != default)
            {
                item.DateCreated = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateCreated, timeZone);
            }

            if (item.DateUpdated != default)
            {
                item.DateUpdated = _appTimeZonesHelper.ConvertFromUtcToTimeZone(item.DateUpdated, timeZone);
            }
        }

        public void ConvertFromTimeZoneToUtc(string timeZone, IHasDateTimeStartAndEnd item)
        {
            if (item == null)
            {
                return;
            }

            item.DateTimeStart = _appTimeZonesHelper.ConvertFromTimeZoneToUtc(item.DateTimeStart, timeZone);

            if (item.DateTimeEnd.HasValue)
            {
                item.DateTimeEnd = _appTimeZonesHelper.ConvertFromTimeZoneToUtc(item.DateTimeEnd.Value, timeZone);
            }
        }
    }
}