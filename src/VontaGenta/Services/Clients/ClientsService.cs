using System.Data;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Mapping;

namespace VontaGenta.Services
{
    public class ClientsService : IClientsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientsService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetClientsDTO> GetClientsForOrg(int activeOrgId, string userId, CancellationToken ct)
        {
            var dto = new GetClientsDTO();

            await _unitOfWork.BeginTransactionAsync(activeOrgId, IsolationLevel.ReadCommitted, ct);

            try
            {
                dto.List = await _unitOfWork.Clients.GetClientsForOrg(activeOrgId, userId, ct);
                await _unitOfWork.CommitAsync(ct);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.AppError = "Error on getting clients";
            }

            return dto;
        }

        public async Task<GetClientDTO> GetClientAndRelatedFields(int activeOrgId, int id, CancellationToken ct)
        {
            var dto = new GetClientDTO();

            await _unitOfWork.BeginTransactionAsync(activeOrgId, IsolationLevel.ReadCommitted, ct);

            try
            {
                dto.Client = await _unitOfWork.Clients.GetClientAndRelatedFields(id, ct);

                if (dto.Client == null)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.AppError = "Error: Not found";
                    return dto;
                }

                await _unitOfWork.CommitAsync(ct);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.AppError = "Error on getting client";
            }

            return dto;
        }

        public async Task<SaveClientDTO> SaveClient(int activeOrgId, Client client, string userId, CancellationToken ct)
        {
            var dto = new SaveClientDTO();

            if (client.AppUserID != userId)
            {
                dto.AppError = "Error: Cannot create clients for other users";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(activeOrgId, IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Clients.SaveClient(client, userId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item saved";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.AppError = "Save failed";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.AppError = "Error on save";
                return dto;
            }

            return dto;
        }

        public async Task<UpdateClientDTO> UpdateClient(int activeOrgId, Client client, string userId, CancellationToken ct)
        {
            var dto = new UpdateClientDTO();

            await _unitOfWork.BeginTransactionAsync(activeOrgId, IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Clients.UpdateClient(client, userId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.AppError = "Error: Error on update";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.AppError = "Error: Error on update";
                return dto;
            }

            return dto;
        }

        public async Task<DeleteClientDTO> DeleteClient(int activeOrgId, int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteClientDTO();

            await _unitOfWork.BeginTransactionAsync(activeOrgId, IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Clients.DeleteClient(id, userId, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item deleted";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.AppError = "Failed to delete item. There might be existing data associated to this item which is blocking the deletion.";
                    return dto;
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.AppError = "Error: Error on delete";
                return dto;
            }

            return dto;
        }

    }
}
