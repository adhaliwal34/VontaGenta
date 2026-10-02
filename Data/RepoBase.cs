using Npgsql;
using VontaGenta.Helpers;

namespace VontaGenta.Data
{
    public abstract class RepoBase : ErrorHelpers
    {
        protected readonly IUnitOfWork _uow;
        protected readonly ILogger _logger;

        protected RepoBase(IUnitOfWork uow, ILogger logger)
        {
            _uow = uow;
            _logger = logger;
        }

        protected NpgsqlConnection Connection => _uow.Connection;
        protected NpgsqlTransaction? Transaction => _uow.Transaction;
    }
}
