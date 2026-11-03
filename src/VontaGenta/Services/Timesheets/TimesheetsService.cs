using System.Data;
using System.Globalization;
using VontaGenta.Data;
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public class TimesheetsService : ITimesheetsService
    {
        private readonly IAppTimeZonesService _appTimeZonesService;
        private readonly IUnitOfWork _unitOfWork;

        public TimesheetsService(IAppTimeZonesService appTimeZonesService,
            IUnitOfWork unitOfWork)
        {
            _appTimeZonesService = appTimeZonesService;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetTimesheetsDTO> GetTimesheets(DateTime? fromDate, string timeZoneId, string userId, int orgId, CancellationToken ct)
        {
            var dto = new GetTimesheetsDTO
            {
                FromDate = fromDate
            };

            if (dto.FromDate == null || dto.FromDate == DateTime.MinValue)
            {
                var today = DateTime.UtcNow;
                dto.FromDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
            }

            var to = dto.FromDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);

            dto.List = (await _unitOfWork.Timesheets.GetTimesheetsAndRelatedFieldsForMemberOrg(dto.FromDate.Value, to, userId, orgId, ct)).ToList();

            foreach (var timesheet in dto.List)
            {
                _appTimeZonesService.ConvertFromUtcToTimeZone(timeZoneId, timesheet);

                var taskDuration = timesheet.GetDuration();

                if (taskDuration != null)
                {
                    PopulateTotalTimePerProject(dto.TotalTimePerProject, timesheet, taskDuration.Value);

                    // TODO: document that the current implementation only displays hours and minutes up to a maximum of 23 hours and 59 minutes, days etc not supported.
                    dto.TimesheetDuration.Add(timesheet.ID, taskDuration.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture));
                }
            }

            return dto;
        }

        private void PopulateTotalTimePerProject(Dictionary<string, TimeSpan> totalTime, Timesheet timesheet, TimeSpan taskDuration)
        {
            var projectIdName = $"{timesheet.Project?.ID}:{timesheet.Project?.Name}";

            totalTime[projectIdName] = totalTime.GetValueOrDefault(projectIdName) + taskDuration;
        }

        /*      public TaskServiceResponse EndPreviousTask(AppUser user)
        {
            var now = DateTime.UtcNow;
            // End the previous timesheet if possible. Only search for a timesheet on the current UTC date.
            var prev = _unitOfWork.Timesheets.GetClosestOpenPreviousTask(now, user.Id);

            if (prev == null)
            {
                return new TaskServiceResponse { Result = Result.NotFound };
            }

            if (!_timesheetAuthorisation.CanEdit(user, prev))
            {
                return new TaskServiceResponse { Result = Result.NotAuthorised };
            }

            prev.DateTimeEnd = now;

            var errors = prev.Validate();

            if (errors.Any())
            {
                return new TaskServiceResponse { Result = Result.FailedValidation, ValidationErrors = errors.GetErrors() };
            }

            try
            {
                _unitOfWork.Timesheets.UpdateTask(prev);
                _unitOfWork.Timesheets.Save();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new TaskServiceResponse { Result = Result.Failure, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new TaskServiceResponse { Result = Result.Failure, Message = ex.Message };
            }

            return new TaskServiceResponse { Result = Result.Success };
        }
        */

        public async Task<GetTimesheetDTO> GetTimesheetById(int? id, string userId, CancellationToken ct)
        {
            var dto = new GetTimesheetDTO();

            if (id == null || id <= 0)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Timesheets.AllowGetTimesheetIfOrgMember(id.Value, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Timesheet = await _unitOfWork.Timesheets.GetTimesheetById(id.Value, ct);

            if (dto.Timesheet == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            return dto;
        }

        public async Task<GetTimesheetDTO> GetTimesheetAndRelatedFieldsById(int? id, string userId, string timeZoneId, CancellationToken ct)
        {
            var dto = new GetTimesheetDTO();

            if (id == null || id <= 0)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Timesheets.AllowGetTimesheetIfOrgMember(id.Value, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            dto.Timesheet = await _unitOfWork.Timesheets.GetTimesheetAndRelatedFields(id.Value, ct);

            if (dto.Timesheet == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            _appTimeZonesService.ConvertFromUtcToTimeZone(timeZoneId, dto.Timesheet);

            return dto;
        }

        public GetTimesheetDTO GetNewTimesheet(string userId, string timeZoneId, DateTime? start)
        {
            Timesheet timesheet;

            try
            {
                timesheet = new Timesheet();

                var now = start.HasValue ? start.Value : DateTime.UtcNow;
                now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Unspecified);  // necessary to discard the milliseconds and the Z letter.
                timesheet.DateTimeStart = now;
                timesheet.AppUserID = userId;

                _appTimeZonesService.ConvertFromUtcToTimeZone(timeZoneId, timesheet);
            }
            catch (Exception ex)
            {
                return new GetTimesheetDTO { Error = ex.Message };
            }

            return new GetTimesheetDTO { Timesheet = timesheet };
        }

        public async Task<SaveTimesheetDTO> SaveTimesheet(Timesheet timesheet, string userId, string timeZoneId, CancellationToken ct)
        {
            var dto = new SaveTimesheetDTO();

            try
            {
                timesheet.AppUserID = userId;

                if (!await _unitOfWork.Timesheets.AllowCreateTimesheet(timesheet.ProjectID, userId, ct))
                {
                    dto.Error = "Failed to save item";
                    return dto;
                }

                _appTimeZonesService.ConvertFromTimeZoneToUtc(timeZoneId, timesheet);

                var errors = timesheet.Validate();

                if (errors.Any())
                {
                    dto.ErrorList = errors.GetErrors();
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.Timesheets.SaveTimesheet(timesheet, ct);

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
                dto.Error = "Error on save";
                // TODO log error
                return dto;
            }

            return dto;
        }

        public async Task<UpdateTimesheetDTO> UpdateTimesheet(Timesheet updatedTimesheet, string userId, string timeZoneId, CancellationToken ct)
        {
            var dto = new UpdateTimesheetDTO();

            try
            {
                _appTimeZonesService.ConvertFromTimeZoneToUtc(timeZoneId, updatedTimesheet);

                if (!await _unitOfWork.Timesheets.AllowUpdateTimesheet(updatedTimesheet.ID, userId, ct))
                {
                    dto.Error = "Error: Not found";
                    return dto;
                }

                var errors = updatedTimesheet.Validate();

                if (errors.Any())
                {
                    dto.ErrorList = errors.GetErrors();
                    dto.Error = "Error on update";
                    return dto;
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                var affectedRows = await _unitOfWork.Timesheets.UpdateTimesheet(updatedTimesheet, ct);

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
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(ct);
                dto.Error = "Error on update";
                // TODO log error
                return dto;
            }

            return dto;
        }

        public async Task<DeleteTimesheetDTO> DeleteTimesheet(int? id, string userId, CancellationToken ct)
        {
            var dto = new DeleteTimesheetDTO();

            if (id == null || id <= 0)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            if (!await _unitOfWork.Timesheets.AllowUpdateTimesheet(id.Value, userId, ct))
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            var timesheet = await _unitOfWork.Timesheets.GetTimesheetById(id.Value, ct);

            if (timesheet == null)
            {
                dto.Error = "Error: Not found";
                return dto;
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                int affectedRows = await _unitOfWork.Timesheets.DeleteTimesheet(timesheet, ct);

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
                //TODO log error
                dto.Error = "Error on delete";
                return dto;
            }

            return dto;
        }
    }
}
