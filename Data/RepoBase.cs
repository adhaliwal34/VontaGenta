using VontaGenta.Helpers;
using MySqlConnector;

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

        protected MySqlConnection Connection => _uow.Connection;
        protected MySqlTransaction? Transaction => _uow.Transaction;
    }
}