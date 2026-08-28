using System.Data;
using VontaGenta.Data;
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class RevenuesService : IRevenuesService
    {
        private readonly IAppTimeZonesService _appTimeZonesService;
        private readonly IUnitOfWork _unitOfWork;

        public RevenuesService(IAppTimeZonesService appTimeZonesService,
            IUnitOfWork unitOfWork)
        {
            _appTimeZonesService = appTimeZonesService;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetRevenuesDTO> GetRevenues(DateTime? date, string timeZoneId, string userId, int orgId, CancellationToken ct)
        {
            var dto = new GetRevenuesDTO
            {
                Date = date
            };

            if (dto.Date == null || dto.Date == DateTime.MinValue)
            {
                var todayUserTz = _appTimeZonesService.ConvertFromUtcToTimeZone(timeZoneId, DateTime.UtcNow);
                // Use midnight on date. Not interested in time.
                dto.Date = new DateTime(todayUserTz.Year, todayUserTz.Month, todayUserTz.Day, 0, 0, 0);
            }

            // Get any revenue on Date.
            dto.List = (await _unitOfWork.Revenues.GetRevenuesAndRelatedFields(dto.Date.Value, userId, orgId, ct)).ToList();

            return dto;
        }

        public async Task<GetRevenueDTO> GetRevenueById(int? id, string userId, CancellationToken ct)
        {
            var dto = new GetRevenueDTO();

            if (id == null || id <= 0)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Revenues.CanGetRevenue(id.Value, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Revenue = await _unitOfWork.Revenues.GetRevenueById(id.Value, ct);

            if (dto.Revenue == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<GetRevenueDTO> GetRevenueAndRelatedFieldsById(int? id, string userId, string timeZoneId, CancellationToken ct)
        {
            var dto = new GetRevenueDTO();

            if (id == null || id <= 0)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Revenues.CanGetRevenue(id.Value, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Revenue = await _unitOfWork.Revenues.GetRevenueAndRelatedFields(id.Value, ct);

            if (dto.Revenue == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public GetRevenueDTO GetNewRevenue(string userId, string timeZoneId, DateTime? date)
        {
            Revenue revenue;

            try
            {
                revenue = new Revenue();

                revenue.AppUserID = userId;

                if (date != null)
                {
                    revenue.Date = date.Value;
                }
                else
                {
                    var todayUserTz = _appTimeZonesService.ConvertFromUtcToTimeZone(timeZoneId, DateTime.UtcNow);
                    // Use midnight on date. Not interested in time.
                    revenue.Date = new DateTime(todayUserTz.Year, todayUserTz.Month, todayUserTz.Day, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                return new GetRevenueDTO { Error = ex.Message };
            }

            return new GetRevenueDTO { Revenue = revenue };
        }


        public async Task<SaveRevenueDTO> SaveRevenue(Revenue revenue, string userId, CancellationToken ct)
        {
            var dto = new SaveRevenueDTO();

            try
            {
                revenue.AppUserID = userId;

                var errors = revenue.Validate();

                if (errors.Any())
                {
                    dto.ErrorList = errors.GetErrors();
                    dto.Error = "Error on save";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Revenues.SaveRevenue(revenue, ct);

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
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on save";
                return dto;
            }

            return dto;
        }


        public async Task<UpdateRevenueDTO> UpdateRevenue(Revenue updatedRevenue, string userId, CancellationToken ct)
        {
            var dto = new UpdateRevenueDTO();

            try
            {
                if (!await _unitOfWork.Revenues.CanGetRevenue(updatedRevenue.ID, userId, ct))
                {
                    dto.Error = "Error: Not found";
                    return dto;
                }

                var errors = updatedRevenue.Validate();

                if (errors.Any())
                {
                    dto.ErrorList = errors.GetErrors();
                    dto.Error = "Error on update";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Revenues.UpdateRevenue(updatedRevenue, ct);

                if (affectedRows > 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    dto.Status = "Item updated";
                }
                else
                {
                    await _unitOfWork.RollbackAsync(ct);
                    dto.Error = "Update failed";
                    return dto;
                }
            }
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on update";
                return dto;
            }

            return dto;
        }


        public async Task<DeleteRevenueDTO> DeleteRevenue(int? id, string userId, CancellationToken ct)
        {
            var dto = new DeleteRevenueDTO();

            try
            {
                if (id == null || id <= 0)
                {
                    dto.Error = "Error: Not found";
                    return dto;
                }

                if (!await _unitOfWork.Revenues.CanGetRevenue(id.Value, userId, ct))
                {
                    dto.Error = "Error: Not found";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Revenues.DeleteRevenue(id.Value, ct);

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
            catch(Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                //TODO log error
                dto.Error = "Error on delete";
                return dto;
            }

            return dto;
        }
    }
}
