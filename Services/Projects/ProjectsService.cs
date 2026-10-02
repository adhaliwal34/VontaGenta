using System.Data;
using AutoMapper;
using VontaGenta.Data;
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class ProjectsService : IProjectsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectsService(IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetProjectsDTO> GetProjectsForMemberOrg(string userId, int orgId, CancellationToken ct)
        {
            var dto = new GetProjectsDTO();

            dto.List = await _unitOfWork.Projects.GetProjectsForMemberOrg(userId, orgId, ct);

            return dto;
        }

        public async Task<GetProjectDTO> GetProject(int id, string userId, CancellationToken ct)
        {
            var dto = new GetProjectDTO();

            if (!await _unitOfWork.Projects.AllowGetProject(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Project = await _unitOfWork.Projects.GetProject(id, ct);

            if (dto.Project == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<GetProjectDTO> GetProjectAndRelatedFields(int id, string userId, CancellationToken ct)
        {
            var dto = new GetProjectDTO();

            if (!await _unitOfWork.Projects.AllowGetProject(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Project = await _unitOfWork.Projects.GetProjectAndRelatedFields(id, ct);

            if (dto.Project == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<SaveProjectDTO> SaveProject(Project project, string userId, CancellationToken ct)
        {
            var dto = new SaveProjectDTO();

            if (project.AppUserID != userId)
            {
                dto.Error = "Error: Cannot create projects for other users";
                return dto;
            }

            if (!await _unitOfWork.Projects.AllowCreateProject(project.ClientID, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Projects.SaveProject(project, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item saved";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Save failed";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on save";
                return dto;
            }

            return dto;
        }

        public async Task<UpdateProjectDTO> UpdateProject(int id, Project project, string userId, CancellationToken ct)
        {
            var dto = new UpdateProjectDTO();

            if (!await _unitOfWork.Projects.AllowGetProject(project.ID, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var storedProject = await _unitOfWork.Projects.GetProject(id, ct);

            if (storedProject == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            _mapper.Map(project, storedProject);

            if (!await _unitOfWork.Projects.AllowUpdateProject(storedProject.ID, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                var result = await _unitOfWork.Projects.UpdateProject(storedProject, ct);

                if (result > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Error: Error on update";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                // TODO log error
                dto.Error = "Error: Error on update";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteProjectDTO> DeleteProject(int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteProjectDTO();

            if (!await _unitOfWork.Projects.AllowGetProject(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var project = await _unitOfWork.Projects.GetProject(id, ct);

            if (project == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Projects.AllowUpdateProject(project.ID, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Projects.DeleteProject(project, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item deleted";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Failed to delete item. There might be existing data associated to this item which is blocking the deletion.";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                // TODO log error
                dto.Error = "Error: Error on update";
                return dto;
            }

            return dto;
        }

    }
}
