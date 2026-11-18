using Npgsql;
using VontaGenta.Helpers;

namespace VontaGenta.Data
{
    public abstract class RepoBase : ErrorHelpers
    {
        private readonly IUnitOfWork _uow;

        protected RepoBase(IUnitOfWork uow, ILogger logger)
        {
            _uow = uow;
            Logger = logger;
        }

        protected ILogger Logger { get; }
        protected NpgsqlConnection Connection => _uow.Connection;
        protected NpgsqlTransaction? Transaction => _uow.Transaction;
    }
}
