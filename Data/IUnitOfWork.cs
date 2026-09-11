using Npgsql;
using System.Data;

namespace VontaGenta.Data
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IClientsRepo Clients { get; }
        IProjectsRepo Projects { get; }
        ICostEntriesRepo CostEntries { get; }
        IAppTimeZonesRepo AppTimeZones { get; }
        ICostCategoriesRepo CostCategories { get; }
        ICurrenciesRepo Currencies { get; }
        IOrgInvitesRepo OrgInvites { get; }
        IOrgsRepo Orgs { get; }
        IRevenuesRepo Revenues { get; }
        ITimesheetsRepo Timesheets { get; }
        IUserOrgsRepo UserOrgs { get; }

        NpgsqlConnection Connection { get; }
        // Null because not all queries require a transaction
        NpgsqlTransaction? Transaction { get; }

        Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
