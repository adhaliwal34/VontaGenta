using VontaGenta.Entities;
using MySqlConnector;
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
                        Projects.*
                        ,AspNetUsers.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Projects
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Projects.AppUserID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE UserOrgs.AppUserID = @AppUserId
                            AND Orgs.ID = @OrgID
                        ORDER BY Projects.StartDate;";

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
                        Projects.*
                        ,AspNetUsers.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Projects
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Projects.AppUserID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                        WHERE UserOrgs.AppUserID = @AppUserId
                            AND Orgs.ID = @OrgID
                        ORDER BY Projects.StartDate;";

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
                        Projects.*
                        FROM Projects
                        WHERE Projects.ID = @Id;";

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
                        Projects.*
                        ,AspNetUsers.*
                        ,Clients.*
                        ,Orgs.*
                        FROM Projects
                        INNER JOIN AspNetUsers ON AspNetUsers.ID = Projects.AppUserID
                        INNER JOIN Clients ON Clients.ID = Projects.ClientID
                        INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                        WHERE Projects.ID = @Id;";

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

        public async Task<int> SaveProject(Project project, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.Name, project.Description, project.StartDate, project.EndDate, project.AppUserID, project.ClientID };
                var sql = @"INSERT Projects (Name, Description, StartDate, EndDate, AppUserID, ClientID)
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

        public async Task<int> UpdateProject(Project project, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.ID, project.Name, project.Description, project.StartDate, project.EndDate, project.ClientID };
                var sql = @"UPDATE Projects
                            SET Name = @Name, Description = @Description, StartDate = @StartDate, EndDate = @EndDate, ClientID = @ClientID
                            WHERE Projects.ID = @ID;";

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

        public async Task<int> DeleteProject(Project project, CancellationToken ct, MySqlTransaction? tx = null)
        {
            if (project == null)
            {
                return 0;
            }

            try
            {
                var tran = tx ?? Transaction;

                var parameters = new { project.ID };
                var sql = @"DELETE FROM Projects WHERE Projects.ID = @ID;";

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
                            1
                            FROM Projects
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE UserOrgs.AppUserID = @AppUserId
                                AND Projects.ID = @ProjectId;";

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
                            1
                            FROM Clients
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE Clients.ID = @ClientId
                                AND UserOrgs.AppUserID = @AppUserId;";

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
                            1
                            FROM Projects
                            INNER JOIN Clients ON Clients.ID = Projects.ClientID
                            INNER JOIN Orgs ON Orgs.ID = Clients.OrgID
                            INNER JOIN UserOrgs ON UserOrgs.OrgID = Clients.OrgID
                            WHERE Projects.ID = @ProjectId
                                AND Projects.AppUserID = @AppUserId
                                AND UserOrgs.AppUserID = @AppUserId;";

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
