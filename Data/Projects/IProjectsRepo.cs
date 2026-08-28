using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface IProjectsRepo
    {
        Task<List<Project>> GetProjectsForMemberOrg(string userId, int orgId, CancellationToken ct);
        Task<List<Project>> GetProjectsForOrg(string userId, int orgId, CancellationToken ct);
        Task<Project?> GetProject(int id, CancellationToken ct);
        Task<Project?> GetProjectAndRelatedFields(int id, CancellationToken ct);
        Task<int> SaveProject(Project project, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateProject(Project project, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> DeleteProject(Project project, CancellationToken ct, MySqlTransaction? tx = null);
        Task<bool> AllowGetProject(int projectId, string userId, CancellationToken ct);
        Task<bool> AllowCreateProject(int clientId, string userId, CancellationToken ct);
        Task<bool> AllowUpdateProject(int projectId, string userId, CancellationToken ct);
    }
}