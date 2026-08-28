using VontaGenta.Data;

namespace VontaGenta.Services
{
    public class CurrenciesService : ICurrenciesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CurrenciesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCurrenciesDTO> GetCurrencies(CancellationToken ct)
        {
            var dto = new GetCurrenciesDTO();

            dto.List = await _unitOfWork.Currencies.GetCurrencies(ct);

            return dto;
        }

        public async Task<GetCurrencyDTO> GetCurrency(int id, CancellationToken ct)
        {
            var dto = new GetCurrencyDTO();

            dto.Currency = await _unitOfWork.Currencies.GetCurrency(id, ct);

            if (dto.Currency == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

    }
}
