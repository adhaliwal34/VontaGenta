using Dapper;
using Npgsql;
using VontaGenta.Entities;

namespace VontaGenta.Data
{
    public class CostCategoriesRepo : RepoBase, ICostCategoriesRepo
    {

        public CostCategoriesRepo(IUnitOfWork uow, ILogger<CostCategoriesRepo> logger)
         : base(uow, logger)
        {
            _logger.LogInformation("CostCategoriesRepo instantiated");
        }

        public async Task<List<Category>> GetCategoriesForMemberOrg(string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                        costcategories.*
                        ,orgs.*
                        ,aspnetusers.*
                        FROM costcategories
                        INNER JOIN orgs ON orgs.id = costcategories.orgid
                        INNER JOIN aspnetusers ON aspnetusers.id = costcategories.appuserid
                        INNER JOIN userorgs ON userorgs.orgid = costcategories.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND orgs.id = @OrgId
                        ORDER BY costcategories.name;";


                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct
                                );

                return (await Connection.QueryAsync<Category, Org, AppUser, Category>(command, (category, org, appUser) =>
                {
                    category.Org = org;
                    category.User = appUser;
                    return category;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Category>();
            }
        }

        public async Task<Category?> GetCategory(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                            costcategories.*
                            ,orgs.*
                            ,aspnetusers.*
                            FROM costcategories
                            INNER JOIN orgs ON orgs.id = costcategories.orgid
                            INNER JOIN aspnetusers ON aspnetusers.id = costcategories.appuserid
                            WHERE costcategories.id = @Id;";

                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct
                                );

                return (await Connection.QueryAsync<Category, Org, AppUser, Category>(command, (category, org, appUser) =>
                {
                    category.User = appUser;
                    category.Org = org;
                    return category;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveCategory(Category category, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (category == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { category.Name, category.Description, AppUserId = category.AppUserID, OrgId = category.OrgID };
                var sql = @"INSERT INTO costcategories (name, description, appuserid, orgid)
                            SELECT @Name, @Description, @AppUserId, @OrgId;";


                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        public async Task<int> UpdateCategory(Category category, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (category == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                // We don't want to change the existing Org or User.
                var parameters = new { Id = category.ID, category.Name, category.Description };
                var sql = @"UPDATE costcategories
                            SET name = @Name, description = @Description
                            WHERE id = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        public async Task<int> DeleteCategory(Category category, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (category == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { Id = category.ID };
                var sql = @"DELETE FROM costcategories WHERE id = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct,
                    transaction: tran
                );

                return await Connection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return 0;
            }
        }

        #region Permissions

        public async Task<bool> AllowGetCostCategory(int categoryId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { CategoryId = categoryId, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM costcategories
                            INNER JOIN userorgs ON userorgs.orgid = costcategories.orgid
                            WHERE userorgs.appuserid = @AppUserId
                                AND costcategories.id = @CategoryId;";

                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct
                                );

                return (await Connection.QueryAsync<bool>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        public async Task<bool> AllowCreateCostCategory(Category category, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { OrgId = category.OrgID, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM userorgs
                            WHERE userorgs.appuserid = @AppUserId
                                AND userorgs.orgid = @OrgId;";

                var command = new CommandDefinition(
                                    sql,
                                    parameters,
                                    cancellationToken: ct
                                );

                return (await Connection.QueryAsync<bool>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return false;
            }
        }

        #endregion
    }
}
// cSpell:ignore tran
