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
    public class ClientsController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IClientsService _clientsService;
        private readonly IMapper _mapper;

        public ClientsController(IAppUserRepo appUserRepo,
            IClientsService clientsService,
            IUserOrgsService userOrgsService,
            IMapper mapper) : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
            _clientsService = clientsService;
            _mapper = mapper;
        }

        // GET: Clients
        public async Task<IActionResult> Index(ClientIndexViewModel vm, CancellationToken ct)
        {
            // This action is often redirected to with status messages and should maintain that data.
            var existingStatus = GetStatus();

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            var dto = await _clientsService.GetClientsForMemberOrg(activeOrg.ID, user.Id, ct);

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
                vm.List.Add(_mapper.Map<Client, ClientDetailsViewModel>(item));
            }

            return View(vm);
        }

        // GET: Clients/Details/5
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

            var dto = await _clientsService.GetClientAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new ClientDetailsViewModel();

            _mapper.Map(dto.Client, vm);

            return View(vm);
        }

        // GET: Clients/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new ClientFormViewModel();

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            return View(vm);
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientFormViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var client = new Client
                {
                    AppUserID = user.Id,
                    OrgID = activeOrg.ID
                };

                _mapper.Map(vm, client);

                var dto = await _clientsService.SaveClient(client, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    return View(vm);
                }

                SetStatus(dto.Status);
            }
            catch (Exception)
            {
                //TODO log error
                SetError("Error on create");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
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

            var dto = await _clientsService.GetClientAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new ClientFormViewModel();

            _mapper.Map(dto.Client, vm);

            return View(vm);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientFormViewModel vm, CancellationToken ct)
        {
            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var editedClient = new Client { ID = id };

                _mapper.Map(vm, editedClient);

                var dto = await _clientsService.UpdateClient(id, editedClient, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                    return View(vm);
                }

                SetStatus(dto.Status);
            }
            catch (Exception)
            {
                //TODO log error
                SetError("Error on update");
                return View(vm);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
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

            var dto = await _clientsService.GetClientAndRelatedFields(id.Value, user.Id, ct);

            if (!dto.IsSuccess)
            {
                SetError(dto.Error);
                return RedirectToAction(nameof(Index));
            }

            var vm = new ClientDetailsViewModel();

            _mapper.Map(dto.Client, vm);

            return View(vm);
        }

        // POST: Clients/Delete/5
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

            try
            {
                var dto = await _clientsService.DeleteClient(id, user.Id, ct);

                if (!dto.IsSuccess)
                {
                    SetError(dto.Error);
                }

                SetStatus(dto.Status);
            }
            catch (Exception)
            {
                //TODO log error
                SetError("Error on delete");
                //TODO redirect to delete confirmation page
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
