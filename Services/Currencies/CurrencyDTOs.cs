#nullable disable
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class GetCurrenciesDTO : RequestStatus
    {
        public List<Currency> List { get; set; }

        public GetCurrenciesDTO()
        {
            List = new List<Currency>();
        }
    }

    public class GetCurrencyDTO : RequestStatus
    {
        public Currency Currency { get; set; }
    }

}
