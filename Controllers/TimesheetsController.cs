using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Models;
using VontaGenta.Services;
using System.Text;

namespace VontaGenta.Controllers
{
    [Authorize]
    public class TimesheetsController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly ITimesheetsService _timesheetsService;
        private readonly IProjectsService _projectsService;
        private readonly IAppTimeZonesService _appTimeZonesService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public TimesheetsController(IAppUserRepo appUserRepo,
            ITimesheetsService timesheetsService,
            IProjectsService projectsService,
            IUserOrgsService userOrgsService,
            IAppTimeZonesService appTimeZonesService,
            IMapper mapper,
            IConfiguration config) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _timesheetsService = timesheetsService;
            _projectsService = projectsService;
            _appTimeZonesService = appTimeZonesService;
            _mapper = mapper;
            _config = config;
        }

        // GET: Timesheets
        public async Task<IActionResult> Index(TimesheetIndexViewModel vm, CancellationToken ct)
        {
            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            if (existingStatus != null)
            {
                SetStatus($"{existingStatus}");
                // Don't redirect from here. No sensible place to redirect to.
            }

            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _timesheetsService.GetTimesheets(vm.FilterByDate, user.TimeZone.ZoneId, user.Id, activeOrg.ID, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                // TODO test this. Can error be displayed too?
                return StatusCode(500);
            }

            vm.FilterByDate = dto.FromDate;
            vm.TotalTimePerProject = dto.TotalTimePerProject;

            var userTzNow = _appTimeZonesService.ConvertFromUtcToTimeZone(user.TimeZone.ZoneId, DateTime.UtcNow);
            vm.Today = userTzNow;

            vm.SystemVersion = GetSystemVersion(_config);

            foreach (var timesheet in dto.List)
            {
                var item = new TimesheetDetailsViewModel();
                _mapper.Map(timesheet, item);

                // Coping the duration from the DTO to the view model if an entry exists.
                if (dto.TimesheetDuration.ContainsKey(timesheet.ID))
                {
                    item.Duration = dto.TimesheetDuration[timesheet.ID];
                }

                vm.Timesheets.Add(item);
            }

            return View(vm);
        }

        // GET: Timesheets/Create
        public async Task<IActionResult> Create(DateTime? filterByDate, CancellationToken ct)
        {

            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var utcNow = DateTime.UtcNow;

            // Always use the UTC now time to populate the start time.
            // This will display a sensible time for the user in their time zone. If midnight UTC time is used it could convert to the previous day in the user's time zone. If user's time zone is behind UTC.
            // If user is viewing today's date then use UTC now to populate the start date and time, otherwise populate the start date from the date viewed and leave the time as the UTC now time.
            var vm = new TimesheetCreateViewModel
            {
                FilterByDate = filterByDate ?? utcNow,
                DateStart = filterByDate == null || filterByDate.Value.Date == utcNow.Date ? utcNow : new DateTime(filterByDate.Value.Year, filterByDate.Value.Month, filterByDate.Value.Day, utcNow.Hour, utcNow.Minute, utcNow.Second),
            };

            if (filterByDate == null)
            {
                filterByDate = utcNow;
            }

            var dto = _timesheetsService.GetNewTimesheet(user.Id, user.TimeZone.ZoneId, vm.DateStart);

            if (!dto.IsSuccess) {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            _mapper.Map(dto.Timesheet, vm);

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            return View(vm);
        }

        // POST: Timesheets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,DateStart,TimeStart,DateEnd,TimeEnd,ProjectID,FilterByDate")] TimesheetCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            if (ModelState.IsValid)
            {
                var timesheet = new Timesheet();
                _mapper.Map(vm, timesheet);

                var dto = await _timesheetsService.SaveTimesheet(timesheet, user.Id, user.TimeZone.ZoneId, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    if (dto.ErrorList.Any())
                    {
                        ModelState.AddValidationErrors(dto.ErrorList);
                    }

                    return View(vm);
                }

                SetStatus(dto.Status);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            return View(vm);
        }

        /*// POST: Tasks/EndTask
        // Ends the most recent open task on today's date
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndTask()
        {
            var user = await _appUserRepository.GetUser(HttpContext.User.GetEmail());

            var response = _tasksService.EndPreviousTask(user);

            switch (response.Result)
            {
                case Result.Success:
                case Result.NotFound:
                    break;
                case Result.NotAuthorised:
                    return RedirectToAction("Error", "Home", new { message = "Not authorised." });
                case Result.Failure:
                    return RedirectToAction("Error", "Home", new { message = response.Message });
            }

            return RedirectToAction(nameof(Index));
        }*/

        // GET: Timesheets/Details/5
        public async Task<IActionResult> Details(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _timesheetsService.GetTimesheetAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            var vm = new TimesheetDetailsViewModel();

            _mapper.Map(dto.Timesheet, vm);

            vm.FilterByDate = filterByDate;

            return View(vm);
        }

        // GET: Timesheets/Edit/5
        public async Task<IActionResult> Edit(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _timesheetsService.GetTimesheetAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            // Reuse the create view model as there is bespoke code I don't want to repeat or move elsewhere.
            var vm = new TimesheetCreateViewModel();

            _mapper.Map(dto.Timesheet, vm);

            vm.FilterByDate = filterByDate;

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            return View(vm);
        }

        // POST: Timesheets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,Name,Description,DateStart,TimeStart,DateEnd,TimeEnd,ProjectID,FilterByDate")] TimesheetCreateViewModel vm, CancellationToken ct)
        {

            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            if (ModelState.IsValid)
            {

                var getTimesheetDto = await _timesheetsService.GetTimesheetById(vm.ID, user.Id, ct);

                var updatedTimesheet = new Timesheet();

                _mapper.Map(vm, updatedTimesheet);

                // Since updated Timesheet comes from a view model it doesn't have all the required fields.
                updatedTimesheet.AppUserID = user.Id;

                var dto = await _timesheetsService.UpdateTimesheet(updatedTimesheet, user.Id, user.TimeZone.ZoneId, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    ModelState.AddValidationErrors(dto.ErrorList);

                    return View(vm);
                }

                SetStatus(dto.Status);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            return View(vm);
        }

        // GET: Timesheets/Delete/5
        public async Task<IActionResult> Delete(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _timesheetsService.GetTimesheetAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            var vm = new TimesheetDetailsViewModel();

            _mapper.Map(dto.Timesheet, vm);

            vm.FilterByDate = filterByDate;

            return View(vm);
        }

        // POST: Timesheets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _timesheetsService.DeleteTimesheet(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            SetStatus(dto.Status);
            return RedirectToAction(nameof(Index), new { filterByDate });
        }

        private async Task<string?> PopulateCreateViewModelLists(TimesheetCreateViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var getProjectsDTO = await _projectsService.GetProjectsForMemberOrg(userId, orgId, ct);

            var errors = new StringBuilder();

            if (!getProjectsDTO.IsSuccess)
            {
                errors.AppendFormat("Error getting projects: {0}", getProjectsDTO.Error);
            }
            else
            {
                vm.ProjectList = getProjectsDTO.List;
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }

            return null;
        }

        private async Task<string?> PopulateCreateViewModelSelectLists(TimesheetCreateViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var errors = await PopulateCreateViewModelLists(vm, userId, orgId, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                return errors;
            }

            if (!SetProjectSelectList(vm))
            {
                return "Error setting select list";
            }

            return null;
        }
    }
}
