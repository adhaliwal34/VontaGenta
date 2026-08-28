using MySqlConnector;
using System.Data;

namespace VontaGenta.Data
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly Lazy<IClientsRepo> _lazyClients;
        private readonly Lazy<IProjectsRepo> _lazyProjects;
        private readonly Lazy<ICostEntriesRepo> _lazyCostEntries;
        private readonly Lazy<IAppTimeZonesRepo> _lazyAppTimeZones;
        private readonly Lazy<ICostCategoriesRepo> _lazyCostCategories;
        private readonly Lazy<ICurrenciesRepo> _lazyCurrencies;
        private readonly Lazy<IOrgInvitesRepo> _lazyOrgInvites;
        private readonly Lazy<IOrgsRepo> _lazyOrgs;
        private readonly Lazy<IRevenuesRepo> _lazyRevenues;
        private readonly Lazy<ITimesheetsRepo> _lazyTimesheets;
        private readonly Lazy<IUserOrgsRepo> _lazyUserOrgs;

        private MySqlConnection? _connection;
        private MySqlTransaction? _transaction;

        // _connection set on instantiation of object
        public MySqlConnection Connection => _connection ?? throw new Exception("Connection not found");
        public MySqlTransaction? Transaction => _transaction;

        public IClientsRepo Clients => _lazyClients.Value;
        public IProjectsRepo Projects => _lazyProjects.Value;
        public ICostEntriesRepo CostEntries => _lazyCostEntries.Value;
        public IAppTimeZonesRepo AppTimeZones => _lazyAppTimeZones.Value;
        public ICostCategoriesRepo CostCategories => _lazyCostCategories.Value;
        public ICurrenciesRepo Currencies => _lazyCurrencies.Value;
        public IOrgInvitesRepo OrgInvites => _lazyOrgInvites.Value;
        public IOrgsRepo Orgs => _lazyOrgs.Value;
        public IRevenuesRepo Revenues => _lazyRevenues.Value;
        public ITimesheetsRepo Timesheets => _lazyTimesheets.Value;
        public IUserOrgsRepo UserOrgs => _lazyUserOrgs.Value;

        public UnitOfWork(IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            _connectionString = config["ConnectionStrings:ApplicationDbContext"] ?? throw new Exception("Connection string not found");
            _connection = new MySqlConnection(_connectionString);
            _logger = loggerFactory.CreateLogger<UnitOfWork>();

            // Lazy instantiation — repo only created when .Value is first accessed
            _lazyClients = new Lazy<IClientsRepo>(() =>
                new ClientsRepo(this, loggerFactory.CreateLogger<ClientsRepo>()));

            _lazyProjects = new Lazy<IProjectsRepo>(() =>
                new ProjectsRepo(this, loggerFactory.CreateLogger<ProjectsRepo>()));

            _lazyCostEntries = new Lazy<ICostEntriesRepo>(() =>
                new CostEntriesRepo(this, loggerFactory.CreateLogger<CostEntriesRepo>()));

            _lazyAppTimeZones = new Lazy<IAppTimeZonesRepo>(() =>
                new AppTimeZonesRepo(this, loggerFactory.CreateLogger<AppTimeZonesRepo>()));

            _lazyCostCategories = new Lazy<ICostCategoriesRepo>(() =>
                new CostCategoriesRepo(this, loggerFactory.CreateLogger<CostCategoriesRepo>()));

            _lazyCurrencies = new Lazy<ICurrenciesRepo>(() =>
                new CurrenciesRepo(this, loggerFactory.CreateLogger<CurrenciesRepo>()));

            _lazyOrgInvites = new Lazy<IOrgInvitesRepo>(() =>
                new OrgInvitesRepo(this, loggerFactory.CreateLogger<OrgInvitesRepo>()));

            _lazyOrgs = new Lazy<IOrgsRepo>(() =>
                new OrgsRepo(this, loggerFactory.CreateLogger<OrgsRepo>()));

            _lazyRevenues = new Lazy<IRevenuesRepo>(() =>
                new RevenuesRepo(this, loggerFactory.CreateLogger<RevenuesRepo>()));

            _lazyTimesheets = new Lazy<ITimesheetsRepo>(() =>
                new TimesheetsRepo(this, loggerFactory.CreateLogger<TimesheetsRepo>()));

            _lazyUserOrgs = new Lazy<IUserOrgsRepo>(() =>
                new UserOrgsRepo(this, loggerFactory.CreateLogger<UserOrgsRepo>()));

            _logger.LogInformation("UnitOfWork instantiated");
        }

        public async Task BeginTransactionAsync(
                IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                CancellationToken ct = default)
        {
            if (_connection == null)
            {
                throw new Exception("Connection not found");
            }

            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync(ct);

            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started.");

            _transaction = await _connection.BeginTransactionAsync(isolationLevel, ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.CommitAsync(ct);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync(ct);
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}