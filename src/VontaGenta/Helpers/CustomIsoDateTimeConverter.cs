using Newtonsoft.Json.Converters;

namespace VontaGenta.Helpers
{
    public class CustomIsoDateTimeConverter : IsoDateTimeConverter
    {
        public CustomIsoDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        }
    }
}
