using VontaGenta.Entities;
using Npgsql;
using Dapper;

namespace VontaGenta.Data
{
    public class ProjectsRepo : RepoBase, IProjectsRepo
    {
        private const string EntityName = "Project";

        public ProjectsRepo(IUnitOfWork uow, ILogger<ProjectsRepo> logger)
        : base(uow, logger)
        {
            _logger.LogInformation("ProjectsRepo instantiated");
        }

        /*
         * Get the projects created under an org the user is a member of.
         * If they created a project under an org but now are no longer
         * a member of the org, do not show that project.
         * 
         * Note, the permissions for which projects a user can see are
         * hard-coded into the SQL.
         *
         *
        */
        public async Task<List<Project>> GetProjectsForMemberOrg(string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                        projects.*
                        ,aspnetusers.*
                        ,clients.*
                        ,orgs.*
                        FROM projects
                        INNER JOIN aspnetusers ON aspnetusers.id = projects.appuserid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND orgs.id = @OrgID
                        ORDER BY projects.startdate;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Project, AppUser, Client, Org, Project>(command, (project, appUser, client, org) =>
                {
                    project.User = appUser;
                    client.Org = org;
                    project.Client = client;
                    return project;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Project>();
            }
        }

        public async Task<List<Project>> GetProjectsForOrg(string userId, int orgId, CancellationToken ct)
        {
            try
            {
                var parameters = new { AppUserId = userId, OrgId = orgId };
                var sql = @"SELECT
                        projects.*
                        ,aspnetusers.*
                        ,clients.*
                        ,orgs.*
                        FROM projects
                        INNER JOIN aspnetusers ON aspnetusers.id = projects.appuserid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                        WHERE userorgs.appuserid = @AppUserId
                            AND orgs.id = @OrgID
                        ORDER BY projects.startdate;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Project, AppUser, Client, Org, Project>(command, (project, appUser, client, org) =>
                {
                    project.User = appUser;
                    client.Org = org;
                    project.Client = client;
                    return project;
                })).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return new List<Project>();
            }
        }

        public async Task<Project?> GetProject(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        projects.*
                        FROM projects
                        WHERE projects.id = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Project>(command)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<Project?> GetProjectAndRelatedFields(int id, CancellationToken ct)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = @"SELECT
                        projects.*
                        ,aspnetusers.*
                        ,clients.*
                        ,orgs.*
                        FROM projects
                        INNER JOIN aspnetusers ON aspnetusers.id = projects.appuserid
                        INNER JOIN clients ON clients.id = projects.clientid
                        INNER JOIN orgs ON orgs.id = clients.orgid
                        WHERE projects.id = @Id;";

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct
                );

                return (await Connection.QueryAsync<Project, AppUser, Client, Org, Project>(command, (project, appUser, client, org) =>
                {
                    project.User = appUser;
                    client.Org = org;
                    project.Client = client;
                    return project;
                })).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {Exception}", ex);
                return null;
            }
        }

        public async Task<int> SaveProject(Project project, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.Name, project.Description, project.StartDate, project.EndDate, project.AppUserID, project.ClientID };
                var sql = @"INSERT INTO projects (name, description, startdate, enddate, appuserid, clientid)
                            SELECT @Name, @Description, @StartDate, @EndDate, @AppUserID, @ClientID;";

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

        public async Task<int> UpdateProject(Project project, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.ID, project.Name, project.Description, project.StartDate, project.EndDate, project.ClientID };
                var sql = @"UPDATE projects
                            SET name = @Name, description = @Description, startdate = @StartDate, enddate = @EndDate, clientid = @ClientID
                            WHERE projects.id = @ID;";

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

        public async Task<int> DeleteProject(Project project, CancellationToken ct, NpgsqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.ID };
                var sql = @"DELETE FROM projects WHERE projects.id = @ID;";

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

        // Allow getting project if user is an org member.
        public async Task<bool> AllowGetProject(int projectId, string userId, CancellationToken ct)
        {
            try
            {

                var parameters = new { ProjectId = projectId, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM projects
                            INNER JOIN clients ON clients.id = projects.clientid
                            INNER JOIN orgs ON orgs.id = clients.orgid
                            INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                            WHERE userorgs.appuserid = @AppUserId
                                AND projects.id = @ProjectId;";

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

        // Allow create project if user is an org member.
        public async Task<bool> AllowCreateProject(int clientId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { ClientId = clientId, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM clients
                            INNER JOIN orgs ON orgs.id = clients.orgid
                            INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                            WHERE clients.id = @ClientId
                                AND userorgs.appuserid = @AppUserId;";

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

        // Allow update project if user is an org member and is project creator.
        public async Task<bool> AllowUpdateProject(int projectId, string userId, CancellationToken ct)
        {
            try
            {
                var parameters = new { ProjectId = projectId, AppUserId = userId };
                var sql = @"SELECT
                            true
                            FROM projects
                            INNER JOIN clients ON clients.id = projects.clientid
                            INNER JOIN orgs ON orgs.id = clients.orgid
                            INNER JOIN userorgs ON userorgs.orgid = clients.orgid
                            WHERE projects.id = @ProjectId
                                AND projects.appuserid = @AppUserId
                                AND userorgs.appuserid = @AppUserId;";

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
