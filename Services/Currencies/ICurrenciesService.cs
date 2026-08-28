
namespace VontaGenta.Services
{
    public interface ICurrenciesService
    {
        Task<GetCurrenciesDTO> GetCurrencies(CancellationToken ct);
        Task<GetCurrencyDTO> GetCurrency(int id, CancellationToken ct);
    }
}