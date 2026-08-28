using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface IProjectsService
    {
        Task<DeleteProjectDTO> DeleteProject(int id, string userId, CancellationToken ct);
        Task<GetProjectsDTO> GetProjectsForMemberOrg(string userId, int orgId, CancellationToken ct);
        Task<GetProjectDTO> GetProject(int id, string userId, CancellationToken ct);
        Task<GetProjectDTO> GetProjectAndRelatedFields(int id, string userId, CancellationToken ct);
        Task<SaveProjectDTO> SaveProject(Project project, string userId, CancellationToken ct);
        Task<UpdateProjectDTO> UpdateProject(int id, Project project, string userId, CancellationToken ct);
    }
}