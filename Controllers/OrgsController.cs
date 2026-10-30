using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VontaGenta.Data;
using VontaGenta.Entities;
using VontaGenta.Models;
using VontaGenta.Services;

namespace VontaGenta.Controllers
{
    [Authorize]
    public class OrgsController : BaseController
    {
        private readonly IOrgsService _orgsService;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IMapper _mapper;
        private readonly IOrgInvitesService _orgInvitesService;
        private readonly ICurrenciesService _currenciesService;

        public OrgsController(IOrgsService orgsService,
            IUserOrgsService userOrgsService,
            IAppUserRepo appUserRepo,
            IMapper mapper,
            IOrgInvitesService orgInvitesService,
            ICurrenciesService currenciesService) : base(userOrgsService)
        {
            _orgsService = orgsService;
            _appUserRepo = appUserRepo;
            _mapper = mapper;
            _orgInvitesService = orgInvitesService;
            _currenciesService = currenciesService;
        }

        // GET: Orgs
        public async Task<IActionResult> Index(CancellationToken ct, int error = 0)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var dto = await _orgsService.GetOrgs(user.Id, ct);

            var vm = new OrgIndexViewModel();

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return View(vm);
            }

            var orgInvitesDto = await _orgInvitesService.GetAllInvitesToUser(user.Id, ct);

            if (orgInvitesDto.IsSuccess)
            {
                vm.OrgInvites = orgInvitesDto.OrgInvites;
            }
            else
            {
                SetError(dto.Error);
                // Continue execution. Not necessary to stop display of page.
            }

            foreach (var item in dto.List.Where(t => t.AppUserID == user.Id).ToList())
            {
                vm.OrgsOwned.Add(_mapper.Map<Org, OrgDetailsViewModel>(item));
            }

            foreach (var item in dto.List.Where(t => t.AppUserID != user.Id).ToList())
            {
                vm.OrgsJoined.Add(_mapper.Map<Org, OrgDetailsViewModel>(item));
            }

            if (!dto.ListHasActiveOrg)
            {
                SetStatus("An Organisation must be set as active before proceeding");
            }

            if (error == 1)
            {
                SetError("Active org not found.");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActiveOrg(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            if (_userOrgsService == null)
            {
                SetError("Essential service not found.");
                return RedirectToAction("Index", "Home");
            }

            var dto = await _userOrgsService.SetActiveOrg(user.Id, id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            SetStatus(dto.Status);

            return RedirectToAction(nameof(Index));
        }

        // GET: Orgs/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var dto = await _orgsService.GetOrgAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrgDetailsViewModel();

            _mapper.Map(dto.Org, vm);

            if (_userOrgsService == null)
            {
                SetError("Essential service not found.");
                return RedirectToAction("Index", "Home");
            }

            var orgMembersDto = await _userOrgsService.GetUsersInOrg(dto.Org.ID, ct);

            if (orgMembersDto.IsSuccess)
            {
                vm.OrgMembers = orgMembersDto.List.Select(u => u.User.Name).ToList();
            }
            else
            {
                SetError(orgMembersDto.Error);
            }

            vm.ShowEditLink = dto.Org.AppUserID == user.Id;

            return View(vm);
        }

        // GET: Orgs/Create
        public IActionResult Create()
        {
            return View(new OrgFormViewModel());
        }

        // POST: Orgs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrgFormViewModel vm, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

                var org = new Org();

                _mapper.Map(vm, org);

                org.AppUserID = user.Id;

                var dto = await _orgsService.SaveOrg(org, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    return View(vm);
                }

                SetStatus(dto.Status);
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // GET: Orgs/Edit/5
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

            var dto = await _orgsService.GetOrgAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var currenciesDto = await _currenciesService.GetCurrencies(ct);

            if (!currenciesDto.IsSuccess)
            {
                SetError(currenciesDto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrgEditViewModel();

            //vm.Currencies = string.Join(",", currenciesDto.List.Select(c => c.Name));

            _mapper.Map(dto.Org, vm);

            return View(vm);
        }

        // POST: Orgs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrgEditViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                // UserName is display only and is not posted back, so load it again for the page heading.
                var orgDto = await _orgsService.GetOrgAndRelatedFields(id, user.Id, ct);

                if (!orgDto.IsSuccess)
                {
                    SetError(orgDto.Error);
                    return RedirectToAction(nameof(Index));
                }

                vm.UserName = orgDto.Org.User.Name;

                return View(vm);
            }

            try
            {
                var editedOrg = new Org { ID = id };

                _mapper.Map(vm, editedOrg);

                var dto = await _orgsService.UpdateOrg(id, editedOrg, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                }

                SetStatus(dto.Status);
            }
            catch (Exception)
            {
                SetError("Error on update");
                // TODO log exception
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orgs/Delete/5
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

            var dto = await _orgsService.GetOrgAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrgDetailsViewModel();

            _mapper.Map(dto.Org, vm);

            return View(vm);
        }

        // POST: Orgs/Delete/5
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

            var dto = await _orgsService.DeleteOrg(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            SetStatus(dto.Status);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddInvites(int? id, CancellationToken ct)
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

            var dto = await _orgsService.GetOrg(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrgAddInviteViewModel();

            _mapper.Map(dto.Org, vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInvites(int id, string emailsToInvite, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _orgInvitesService.AddOrgInvites(id, user.Id, emailsToInvite, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }
            else
            {
                SetStatus(dto.Status);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvite(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var dto = await _orgInvitesService.AcceptOrgInvite(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }
            else
            {
                SetStatus(dto.Status);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteInviteConfirmation(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _orgInvitesService.GetOrgInviteToUser(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }

            var vm = new OrgInviteDeleteConfirmationViewModel();

            _mapper.Map(dto.OrgInvite, vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInvite(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _orgInvitesService.DeleteOrgInvite(id, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }
            else
            {
                SetStatus(dto.Status);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orgs/Leave/5
        public async Task<IActionResult> Leave(int? id, CancellationToken ct)
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

            var dto = await _orgsService.GetOrgAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrgDetailsViewModel();

            _mapper.Map(dto.Org, vm);

            return View(vm);
        }

        // POST: Orgs/Leave/5
        [HttpPost, ActionName("Leave")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveConfirmed(int id, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (_userOrgsService == null)
            {
                SetError("Essential service not found.");
                return RedirectToAction("Index", "Home");
            }

            var dto = await _userOrgsService.LeaveOrg(user.Id, id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
            }
            else
            {
                SetStatus(dto.Status);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
// cSpell:ignore overposting
