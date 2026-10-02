using Newtonsoft.Json;

namespace VontaGenta.Helpers
{
    public class DateOnlyIsoConverter : JsonConverter<DateTime>
    {
        public DateOnlyIsoConverter()
        {
            // No DateTimeFormat needed here unless you want it for other purposes
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            // Ignoring time portion
            writer.WriteValue(value.ToString("yyyy-MM-dd"));
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Implement read logic if needed, or throw if read not supported
            return DateTime.Parse(reader.Value?.ToString() ?? throw new JsonSerializationException("Unexpected null"));
        }
    }
}
