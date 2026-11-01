using VontaGenta.Services;

namespace VontaGenta.Tests
{
    public class TimeZonesConversionTests
    {
        [Fact]
        public void ConvertFromUtcToMalaysiaTimeZone()
        {
            // 18 Sept 2025 11 PM UTC
            var utcDateTime = new DateTime(2025, 09, 18, 23, 0, 0, DateTimeKind.Utc);

            var appTimeZonesHelper = new AppTimeZonesHelper();

            // Should convert to 19 Sept 2025 7 AM Malaysia Time (8 hours ahead of UTC)
            var convertedDateTime = appTimeZonesHelper.ConvertFromUtcToTimeZone(utcDateTime, "Asia/Kuala_Lumpur");

            Assert.Equal(new DateTime(2025, 09, 19, 7, 0, 0), convertedDateTime);
        }
    }
}
