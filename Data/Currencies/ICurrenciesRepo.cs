using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public interface ICurrenciesRepo
    {
        Task<List<Currency>> GetCurrencies(CancellationToken ct);
        Task<Currency?> GetCurrency(int currencyId, CancellationToken ct);
    }
}