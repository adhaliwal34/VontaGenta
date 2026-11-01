
using VontaGenta.Entities;

namespace VontaGenta.Services
{
    public interface ITimesheetsService
    {
        Task<GetTimesheetsDTO> GetTimesheets(DateTime? fromDate, string timeZoneId, string userId, int orgId, CancellationToken ct);
        GetTimesheetDTO GetNewTimesheet(string userId, string timeZoneId, DateTime? start);
        Task<SaveTimesheetDTO> SaveTimesheet(Timesheet timesheet, string userId, string timeZoneId, CancellationToken ct);
        Task<UpdateTimesheetDTO> UpdateTimesheet(Timesheet updatedTimesheet, string userId, string timeZoneId, CancellationToken ct);
        Task<GetTimesheetDTO> GetTimesheetById(int? id, string userId, CancellationToken ct);
        Task<GetTimesheetDTO> GetTimesheetAndRelatedFieldsById(int? id, string userId, string timeZoneId, CancellationToken ct);
        Task<DeleteTimesheetDTO> DeleteTimesheet(int? id, string userId, CancellationToken ct);
    }
}
