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

        public async Task<GetClientsDTO> GetClientsForMemberOrg(int orgId, string userId, CancellationToken ct)
        {
            var dto = new GetClientsDTO();

            dto.List = await _unitOfWork.Clients.GetClientsForMemberOrg(orgId, userId, ct);

            return dto;
        }

        public async Task<GetClientDTO> GetClient(int id, string userId, CancellationToken ct)
        {
            var dto = new GetClientDTO();

            if (!await _unitOfWork.Clients.AllowGetClient(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Client = await _unitOfWork.Clients.GetClient(id, ct);

            if (dto.Client == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<GetClientDTO> GetClientAndRelatedFields(int id, string userId, CancellationToken ct)
        {
            var dto = new GetClientDTO();

            if (!await _unitOfWork.Clients.AllowGetClient(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Client = await _unitOfWork.Clients.GetClientAndRelatedFields(id, ct);

            if (dto.Client == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<SaveClientDTO> SaveClient(Client client, string userId, CancellationToken ct)
        {
            var dto = new SaveClientDTO();

            if (client.AppUserID != userId)
            {
                dto.Error = "Error: Cannot create clients for other users";
                return dto;
            }

            if (!await _unitOfWork.Clients.AllowUpdateOrCreateClient(client, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

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

        public async Task<UpdateClientDTO> UpdateClient(int id, Client client, string userId, CancellationToken ct)
        {
            var dto = new UpdateClientDTO();

            if (!await _unitOfWork.Clients.AllowGetClient(client.ID, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var storedClient = await _unitOfWork.Clients.GetClient(id, ct);

            if (storedClient == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            client.MapTo(storedClient);

            if (!await _unitOfWork.Clients.AllowUpdateOrCreateClient(storedClient, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                var result = await _unitOfWork.Clients.UpdateClient(storedClient, userId, ct);

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

        public async Task<DeleteClientDTO> DeleteClient(int id, string userId, CancellationToken ct)
        {
            var dto = new DeleteClientDTO();

            if (!await _unitOfWork.Clients.AllowGetClient(id, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var client = await _unitOfWork.Clients.GetClient(id, ct);

            if (client == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Clients.AllowUpdateOrCreateClient(client, userId, ct))
            {
                dto.Error = "Error: action not allowed";
                return dto;
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            try
            {
                var affectedRows = await _unitOfWork.Clients.DeleteClient(client, userId, ct);

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
