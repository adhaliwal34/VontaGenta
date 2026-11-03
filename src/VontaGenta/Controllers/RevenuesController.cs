using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Mapping;
using VontaGenta.Models;
using VontaGenta.Services;

namespace VontaGenta.Controllers
{
    [Authorize]
    public class RevenuesController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IRevenuesService _revenuesService;
        private readonly IProjectsService _projectsService;
        private readonly IAppTimeZonesService _appTimeZonesService;
        private readonly ICurrenciesService _currenciesService;
        private readonly IConfiguration _config;

        public RevenuesController(IAppUserRepo appUserRepo,
            IRevenuesService revenuesService,
            IProjectsService projectsService,
            IUserOrgsService userOrgsService,
            IAppTimeZonesService appTimeZonesService,
            ICurrenciesService currenciesService,
            IConfiguration config) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _revenuesService = revenuesService;
            _projectsService = projectsService;
            _appTimeZonesService = appTimeZonesService;
            _currenciesService = currenciesService;
            _config = config;
        }

        // GET: Revenues
        public async Task<IActionResult> Index(RevenueIndexViewModel vm, CancellationToken ct)
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

            var dto = await _revenuesService.GetRevenues(vm.FilterByDate, user.TimeZone.ZoneId, user.Id, activeOrg.ID, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                // TODO test this. Can error be displayed too?
                return StatusCode(500);
            }

            vm.FilterByDate = dto.Date;

            var userTzNow = _appTimeZonesService.ConvertFromUtcToTimeZone(user.TimeZone.ZoneId, DateTime.UtcNow);
            vm.Today = userTzNow;

            vm.SystemVersion = GetSystemVersion(_config);

            foreach (var revenue in dto.List)
            {
                var item = new RevenueDetailsViewModel();
                revenue.MapTo(item);
                vm.Revenues.Add(item);

            }

            return View(vm);
        }

        // GET: Revenues/Create
        public async Task<IActionResult> Create(DateTime? filterByDate, CancellationToken ct)
        {

            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var userTzNow = _appTimeZonesService.ConvertFromUtcToTimeZone(user.TimeZone.ZoneId, DateTime.UtcNow);

            // Always use the User's TZ now date to populate the date.
            // This will display a sensible date for the user in their time zone. If midnight UTC time is used it could convert to the previous day in the user's time zone. If user's time zone is behind UTC.
            // If user is viewing today's date then use User's TZ now to populate the date, otherwise populate the date from the date viewed.
            var vm = new RevenueFormViewModel
            {
                FilterByDate = filterByDate ?? userTzNow,
                EntryDate = filterByDate == null || filterByDate.Value.Date == userTzNow.Date ? userTzNow : new DateTime(filterByDate.Value.Year, filterByDate.Value.Month, filterByDate.Value.Day, 0, 0, 0),
            };

            if (filterByDate == null)
            {
                filterByDate = userTzNow;
            }

            var dto = _revenuesService.GetNewRevenue(user.Id, user.TimeZone.ZoneId, vm.EntryDate);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            dto.Revenue.MapTo(vm);

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            return View(vm);
        }

        // POST: Revenues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RevenueFormViewModel vm, CancellationToken ct)
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
                var revenue = new Revenue();
                vm.MapTo(revenue);

                var dto = await _revenuesService.SaveRevenue(revenue, user.Id, ct);

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

        // GET: Revenues/Details/5
        public async Task<IActionResult> Details(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _revenuesService.GetRevenueAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            var vm = new RevenueDetailsViewModel();

            dto.Revenue.MapTo(vm);

            vm.FilterByDate = filterByDate;

            return View(vm);
        }

        // GET: Revenues/Edit/5
        public async Task<IActionResult> Edit(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _revenuesService.GetRevenueAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            // Reusing the create view model for convenience.
            var vm = new RevenueFormViewModel();

            dto.Revenue.MapTo(vm);

            vm.FilterByDate = filterByDate;

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index), new { vm.FilterByDate });
            }

            return View(vm);
        }

        // POST: Revenues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RevenueFormViewModel vm, CancellationToken ct)
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

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var getRevenueDto = await _revenuesService.GetRevenueById(id, user.Id, ct);

            var updatedRevenue = new Revenue { ID = id };

            vm.MapTo(updatedRevenue);

            // Since updated Revenue comes from a view model it doesn't have all the required fields.
            updatedRevenue.AppUserID = user.Id;

            var dto = await _revenuesService.UpdateRevenue(updatedRevenue, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                ModelState.AddValidationErrors(dto.ErrorList);

                return View(vm);
            }

            SetStatus(dto.Status);

            return RedirectToAction(nameof(Index), new { vm.FilterByDate });
        }

        // GET: Revenues/Delete/5
        public async Task<IActionResult> Delete(int? id, DateTime? filterByDate, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUserAndRelatedFields(HttpContext.User.Identity?.Name) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _revenuesService.GetRevenueAndRelatedFieldsById(id, user.Id, user.TimeZone.ZoneId, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index), new { filterByDate });
            }

            var vm = new RevenueDetailsViewModel();

            dto.Revenue.MapTo(vm);

            vm.FilterByDate = filterByDate;

            return View(vm);
        }

        // POST: Revenues/Delete/5
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

            var dto = await _revenuesService.DeleteRevenue(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            SetStatus(dto.Status);

            return RedirectToAction(nameof(Index), new { filterByDate });
        }

        private async Task<string?> PopulateCreateViewModelLists(RevenueFormViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var getProjectsDTO = await _projectsService.GetProjectsForMemberOrg(userId, orgId, ct);

            var errors = new StringBuilder();

            if (!getProjectsDTO.IsSuccess)
            {
                errors.AppendFormat(CultureInfo.InvariantCulture, "Error getting projects: {0}", getProjectsDTO.Error);
            }
            else
            {
                vm.ProjectList = getProjectsDTO.List;
            }

            var getCurrenciesDTO = await _currenciesService.GetCurrencies(ct);

            if (!getCurrenciesDTO.IsSuccess)
            {
                errors.AppendFormat(CultureInfo.InvariantCulture, "Error getting currencies: {0}", getCurrenciesDTO.Error);
            }
            else
            {
                vm.CurrencyList = getCurrenciesDTO.List;
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }

            return null;
        }

        private async Task<string?> PopulateCreateViewModelSelectLists(RevenueFormViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var errors = await PopulateCreateViewModelLists(vm, userId, orgId, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                return errors;
            }

            if (!SetProjectSelectList(vm))
            {
                return "Error setting project select list";
            }

            if (!SetCurrencySelectList(vm))
            {
                return "Error setting currency select list";
            }

            return null;
        }
    }
}
