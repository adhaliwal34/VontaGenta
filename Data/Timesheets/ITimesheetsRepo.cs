using VontaGenta.Entities;
using MySqlConnector;

namespace VontaGenta.Data
{
    public interface ITimesheetsRepo
    {
        Task<Timesheet?> GetClosestOpenPreviousTimesheet(DateTime utcDateTime, string userId, CancellationToken ct);
        Task<Timesheet?> GetTimesheetAndRelatedFields(int id, CancellationToken ct);
        Task<Timesheet?> GetTimesheetById(int id, CancellationToken ct);
        Task<IEnumerable<Timesheet>> GetTimesheetsAndRelatedFieldsForMemberOrg(DateTime fromDate, DateTime toDate, string userId, int orgId, CancellationToken ct);
        Task<int> SaveTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> UpdateTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null);
        Task<int> DeleteTimesheet(Timesheet timesheet, CancellationToken ct, MySqlTransaction? tx = null);
        Task<bool> AllowGetTimesheetIfOrgMember(int timesheetId, string userId, CancellationToken ct);
        Task<bool> AllowCreateTimesheet(int projectId, string userId, CancellationToken ct);
        Task<bool> AllowUpdateTimesheet(int timesheetId, string userId, CancellationToken ct);
    }
}