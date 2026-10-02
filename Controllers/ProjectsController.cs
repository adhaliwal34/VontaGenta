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
    public class ProjectsController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IProjectsService _projectsService;
        private readonly IClientsService _clientsService;
        private readonly IMapper _mapper;

        public ProjectsController(IAppUserRepo appUserRepo,
            IProjectsService projectsService,
            IUserOrgsService userOrgsService,
            IClientsService clientsService,
            IMapper mapper) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _projectsService = projectsService;
            _clientsService = clientsService;
            _mapper = mapper;
        }

        // GET: Projects
        public async Task<IActionResult> Index(ProjectIndexViewModel vm, CancellationToken ct)
        {
            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _projectsService.GetProjectsForMemberOrg(user.Id, activeOrg.ID, ct);

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
                vm.List.Add(_mapper.Map<Project, ProjectDetailsViewModel>(item));
            }

            return View(vm);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _projectsService.GetProjectAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new ProjectDetailsViewModel();

            _mapper.Map(dto.Project, vm);

            return View(vm);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new ProjectCreateViewModel();

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ClientID")] ProjectCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

                // TODO ensure model errors not overwritten.
                if (!string.IsNullOrEmpty(errors))
                {
                    SetError(errors);
                }

                return View(vm);
            }

            var project = new Project
            {
                AppUserID = user.Id
            };

            _mapper.Map(vm, project);

            var dto = await _projectsService.SaveProject(project, user.Id, ct);

            if (!dto.IsSuccess)
            {
                var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

                // TODO ensure model errors not overwritten.
                if (!string.IsNullOrEmpty(errors))
                {
                    SetError(errors);
                }

                SetError(dto.Error);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var vm = new ProjectCreateViewModel();

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                SetError(errors);
                return RedirectToAction(nameof(Index));
            }

            var dto = await _projectsService.GetProjectAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            _mapper.Map(dto.Project, vm);

            return View(vm);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,Name,Description,StartDate,EndDate,ClientID")] ProjectCreateViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var editedProject = new Project();

                    _mapper.Map(vm, editedProject);

                    var dto = await _projectsService.UpdateProject(vm.ID, editedProject, user.Id, ct);

                    if (!dto.IsSuccess)
                    {
                        SetError(dto.Error);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            var errors = await PopulateCreateViewModelSelectLists(vm, user.Id, activeOrg.ID, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                // TODO ensure model errors not overwritten.
                SetError(errors);
            }

            return View(vm);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _projectsService.GetProjectAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new ProjectDetailsViewModel();

            _mapper.Map(dto.Project, vm);

            return View(vm);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var deleteDto = await _projectsService.DeleteProject(id, user.Id, ct);

            if (!deleteDto.IsSuccess)
            {
                SetError(deleteDto.Error);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> PopulateCreateViewModelLists(ProjectCreateViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var getClientsDTO = await _clientsService.GetClientsForMemberOrg(orgId, userId, ct);

            var errors = new StringBuilder();

            if (!getClientsDTO.IsSuccess)
            {
                errors.AppendFormat("Error getting clients: {0}", getClientsDTO.Error);
            }
            else
            {
                vm.ClientList = getClientsDTO.List;
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }

            return null;
        }

        private async Task<string?> PopulateCreateViewModelSelectLists(ProjectCreateViewModel vm, string userId, int orgId, CancellationToken ct)
        {
            var errors = await PopulateCreateViewModelLists(vm, userId, orgId, ct);

            if (!string.IsNullOrEmpty(errors))
            {
                return errors;
            }

            if (!SetClientSelectList(vm))
            {
                return "Error setting client select list";
            }

            return null;
        }
    }
}
