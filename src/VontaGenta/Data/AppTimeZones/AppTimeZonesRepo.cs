using Dapper;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public class AppTimeZonesRepo : RepoBase, IAppTimeZonesRepo
    {
        public AppTimeZonesRepo(IUnitOfWork uow, ILogger<AppTimeZonesRepo> logger)
            : base(uow, logger)
        {
            _logger.LogInformation("AppTimeZonesRepo instantiated");
        }

        public async Task<List<AppTimeZone>> GetTimeZones(CancellationToken ct)
        {
            try
            {
                var sql = @"SELECT apptimezones.* FROM apptimezones";

                var command = new CommandDefinition(
                    sql,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<AppTimeZone>(command)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return [];
            }
        }
    }
}
