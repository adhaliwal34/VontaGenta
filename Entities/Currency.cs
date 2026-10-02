#nullable disable
using System.Globalization;
using Newtonsoft.Json;

namespace VontaGenta.Entities
{
    public class Currency
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }

        [JsonIgnore]
        public string LongName { get { return string.Format(CultureInfo.InvariantCulture, "{0} - {1} - {2}", Name, Code, Symbol); } }
    }
}
