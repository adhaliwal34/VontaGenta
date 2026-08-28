using VontaGenta.Entities;
using Dapper;

namespace VontaGenta.Data
{
    public class CurrenciesRepo : RepoBase, ICurrenciesRepo
    {

        public CurrenciesRepo(IUnitOfWork uow, ILogger<CurrenciesRepo> logger)
        : base (uow, logger)
        {
            _logger.LogInformation("CurrenciesRepo instantiated");
        }

        public async Task<List<Currency>> GetCurrencies(CancellationToken ct)
        {
            try
            {
                var sql = @"SELECT
                            *
                            FROM Currencies
                            ORDER BY Currencies.Name;";

                var command = new CommandDefinition(
                    sql,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Currency>(command)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Currency>();
            }
        }

        public async Task<Currency?> GetCurrency(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                            *
                            FROM Currencies
                            WHERE Currencies.ID = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return await Connection.QueryFirstOrDefaultAsync<Currency>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

    }
}
