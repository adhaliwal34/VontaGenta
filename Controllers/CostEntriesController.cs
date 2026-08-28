using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Helpers;
using VontaGenta.Models;
using VontaGenta.Services;
using System.Text;

namespace VontaGenta.Controllers
{
    [Authorize]
    public class CostEntriesController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly ICostCategoriesService _categoriesService;
        private readonly ICostEntriesService _costEntriesService;
        private readonly IProjectsService _projectsService;
        private readonly ICurrenciesService _currenciesService;
        private readonly IMapper _mapper;

        public CostEntriesController(IAppUserRepo appUserRepo,
            ICostCategoriesService categoriesService,
            ICostEntriesService costEntriesService,
            IProjectsService projectsService,
            IUserOrgsService userOrgsService,
            ICurrenciesService currenciesService,
            IMapper mapper) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _categoriesService = categoriesService;
            _costEntriesService = costEntriesService;
            _projectsService = projectsService;
            _currenciesService = currenciesService;
            _mapper = mapper;
        }

        // GET: CostEntries
        public async Task<IActionResult> Index(CostEntryIndexViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            if (existingStatus != null)
            {
                SetStatus($"{existingStatus}");
                // Don't redirect from here. No sensible place to redirect to.
            }

            if (vm.Year <= 0)
            {
                vm.Year = DateTime.Today.Year;
            }

            if (vm.Month <= 0)
            {
                vm.Month = DateTime.Today.Month;
            }

            return View(vm);
        }

        // GET: CostEntries/List
        public async Task<IActionResult> List(CostEntryListViewModel vm, CancellationToken ct)
        {
            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            if (vm.Year <= 0)
            {
                vm.Year = DateTime.Today.Year;
            }

            if (vm.Month <= 0)
            {
                vm.Month = DateTime.Today.Month;
            }

            vm.MonthsSelectList = new SelectList(new Months().List, "ID", "ShortName", vm.Month);

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var countDto = await _costEntriesService.GetTotalCountCostEntriesForMemberOrg(user.Id, activeOrg.ID, vm.Year, vm.Month, ct);

            if (!countDto.IsSuccess)
            {
                SetError($"{countDto.Error}");
                return RedirectToAction("Index", "Home");
            }

            vm.TotalCount = countDto.Total;

            var dto = await _costEntriesService.GetCostEntriesForMemberOrg(user.Id, activeOrg.ID, vm.Year, vm.Month, vm.Page, vm.ItemsPerPage, ct);

            if (!dto.IsSuccess)
            {
                SetError($"{dto.Error}");
            }

            if (existingStatus != null || dto.Status != null)
            {
                SetStatus($"{existingStatus} {dto.Status}");
                // Don't redirect from here. No sensible place to redirect to.
            }

            foreach (var item in dto.List)
            {
                vm.List.Add(_mapper.Map<CostEntry, CostEntryDetailsViewModel>(item));
            }

            return View(vm);
        }

        // GET: CostEntries/Details/5
        public async Task<IActionResult> Details(int? id, int? year, int? month, int? page, int? itemsPerPage, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _costEntriesService.GetCostEntryAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(List), new { year, month, page, itemsPerPage });
            }

            var vm = new CostEntryDetailsViewModel
            {
                Year = year,
                Month = month,
                Page = page,
                ItemsPerPage = itemsPerPage
            };

            _mapper.Map(dto.CostEntry, vm);

            return View(vm);
        }

        // GET: CostEntries/Create
        public async Task<IActionResult> Create(int? year, int? month, int? page, int? itemsPerPage, CancellationToken ct)
        {
            var vm = new CostEntryCreateViewModel
            {
                Year = year,
                Month = month,
                Page = page,
                ItemsPerPage = itemsPerPage
            };

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(List), new { year, month, page, itemsPerPage });
            }

            return View(vm);
        }

        // POST: CostEntries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Name,Description,CategoryID,Amount,CurrencyID,ProjectID,Year,Month,Page,ItemsPerPage")] CostEntryCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

                if (!string.IsNullOrEmpty(errors))
                {
                    SetError(errors);
                    return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
                }

                // TODO ensure we are not overwriting fields already selected by the user
                return View(vm);
            }

            var costEntry = new CostEntry
            {
                AppUserID = user.Id,
            };

            _mapper.Map(vm, costEntry);

            var dto = await _costEntriesService.SaveCostEntry(costEntry, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);

                // TODO ensure we are not override the error above. Can allow for multiple error messages.
                var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

                if (!string.IsNullOrEmpty(errors))
                {
                    SetError(errors);
                    return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
                }

                // TODO ensure we are not overwriting fields already selected by the user
                return View(vm);
            }

            // Redirect to date of entry
            vm.Year = costEntry.Date.Year;
            vm.Month = costEntry.Date.Month;

            SetStatus("Item saved. Displayed below for you to check...");

            return RedirectToAction(nameof(Details), new { id = dto.SavedID, vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
        }

        // GET: CostEntries/Edit/5
        public async Task<IActionResult> Edit(int? id, int? year, int? month, int? page, int? itemsPerPage, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _costEntriesService.GetCostEntryAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(List), new { year, month, page, itemsPerPage });
            }

            var vm = new CostEntryCreateViewModel
            {
                Year = year,
                Month = month,
                Page = page,
                ItemsPerPage = itemsPerPage
            };

            _mapper.Map(dto.CostEntry, vm);

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
            }

            return View(vm);
        }

        // POST: CostEntries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,Date,Name,Description,CategoryID,ProjectID,Amount,CurrencyID,Year,Month,Page,ItemsPerPage")] CostEntryCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

                if (!string.IsNullOrEmpty(errors))
                {
                    SetError(errors);
                    return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
                }

                return View(vm);
            }

            try
            {
                var editedCostEntry = new CostEntry();

                _mapper.Map(vm, editedCostEntry);

                var dto = await _costEntriesService.UpdateCostEntry(editedCostEntry, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
                }
            }
            catch (Exception)
            {
                // TODO log exception
                return NotFound();
            }

            return RedirectToAction(nameof(List), new { vm.Year, vm.Month, vm.Page, vm.ItemsPerPage });
        }

        // GET: CostEntries/Delete/5
        public async Task<IActionResult> Delete(int? id, int? year, int? month, int? page, int? itemsPerPage, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _costEntriesService.GetCostEntryAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(List), new { year, month, page, itemsPerPage });
            }

            var vm = new CostEntryDetailsViewModel
            {
                Year = year,
                Month = month,
                Page = page,
                ItemsPerPage = itemsPerPage
            };

            _mapper.Map(dto.CostEntry, vm);

            return View(vm);
        }

        // POST: CostEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? year, int? month, int? page, int? itemsPerPage, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var deleteDto = await _costEntriesService.DeleteCostEntry(id, user.Id, ct);

            if (!deleteDto.IsSuccess)
            {
                SetError(deleteDto.Error);
            }
            else
            {
                SetStatus(deleteDto.Status);
            }

            return RedirectToAction(nameof(List), new { year, month, page, itemsPerPage });
        }

        private async Task<string?> PopulateCreateViewModelSelectLists(CostEntryCreateViewModel vm, string userId, int orgId, CancellationToken ct)
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

            if (!SetCategoriesSelectList(vm))
            {
                return "Error setting categories select list";
            }

            if (!SetCurrencySelectList(vm))
            {
                return "Error setting currency select list";
            }

            return null;
        }

        private async Task<string?> PopulateCreateViewModelLists(CostEntryCreateViewModel vm, string userId, int orgId, CancellationToken ct)
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

            var getCategoriesDTO = await _categoriesService.GetCategoriesForMemberOrg(userId, orgId, ct);

            if (!getCategoriesDTO.IsSuccess)
            {
                errors.AppendFormat("Error getting categories: {0}", getProjectsDTO.Error);
            }
            else
            {
                vm.CategoryList = getCategoriesDTO.List;
            }

            var getCurrenciesDTO = await _currenciesService.GetCurrencies(ct);

            if (!getCurrenciesDTO.IsSuccess)
            {
                errors.AppendFormat("Error getting currencies: {0}", getCurrenciesDTO.Error);
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
    }
}
