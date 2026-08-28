using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VontaGenta.Data;
using VontaGenta.Entities;
using AutoMapper;
using VontaGenta.Models;
using VontaGenta.Services;

namespace VontaGenta.Controllers
{
    [Authorize]
    public class CostCategoriesController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IMapper _mapper;
        private readonly ICostCategoriesService _costCategoriesService;

        public CostCategoriesController(IAppUserRepo appUserRepo,
            IMapper mapper,
            ICostCategoriesService costCategoriesService,
            IUserOrgsService userOrgsService) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _mapper = mapper;
            _costCategoriesService = costCategoriesService;
        }

        // GET: CostCategories
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _costCategoriesService.GetCategoriesForMemberOrg(user.Id, activeOrg.ID, ct);

            if (!dto.IsSuccess)
            {
                SetError($"{dto.Error}");
            }

            if (existingStatus != null || dto.Status != null)
            {
                SetStatus($"{existingStatus} {dto.Status}");
                // Don't redirect from here. No sensible place to redirect to.
            }

            var vm = new CostCategoryIndexViewModel();

            // TODO: get org
            // TODO: check that user is the org owner or member
            // TODO: check authorisation based on owner or member status
            // Currently it's true because only the owner can get here.
            vm.CanCreate = vm.CanEditOrDelete = true;

            foreach (var item in dto.List)
            {
                vm.List.Add(_mapper.Map<Category, CostCategoryDetailsViewModel>(item));
            }

            return View(vm);
        }

        // GET: CostCategories/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken ct)
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

            var dto = await _costCategoriesService.GetCategory(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new CostCategoryDetailsViewModel();

            _mapper.Map(dto.Category, vm);

            return View(vm);
        }

        // GET: CostCategories/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var vm = new CostCategoryCreateViewModel();

            return View(vm);
        }

        // POST: CostCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] CostCategoryCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var category = new Category();

            _mapper.Map(vm, category);

            category.AppUserID = user.Id;
            category.OrgID = activeOrg.ID;

            var dto = await _costCategoriesService.SaveCategory(category, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: CostCategories/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken ct)
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

            var dto = await _costCategoriesService.GetCategory(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new CostCategoryEditViewModel();

            _mapper.Map(dto.Category, vm);

            return View(vm);
        }

        // POST: CostCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description")] CostCategoryEditViewModel vm, CancellationToken ct)
        {
            if (id != vm.ID)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new Exception("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var editedCategory = new Category();

            _mapper.Map(vm, editedCategory);

            editedCategory.AppUserID = user.Id;

            var dto = await _costCategoriesService.UpdateCategory(id, editedCategory, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: CostCategories/Delete/5
        public async Task<IActionResult> Delete(int? id, CancellationToken ct)
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

            var dto = await _costCategoriesService.GetCategory(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new CostCategoryDetailsViewModel();

            _mapper.Map(dto.Category, vm);

            return View(vm);
        }

        // POST: CostCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, CancellationToken ct)
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

            var dto = await _costCategoriesService.DeleteCategory(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
