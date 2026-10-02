using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VontaGenta.Services;
using VontaGenta.Data;

namespace VontaGenta.Controllers
{

    [Authorize(Roles = "Administrator")]
    public class AdminController : BaseController
    {
        private readonly IAppUserRepo _appUserRepo;

        public AdminController(IUserOrgsService userOrgsService, IAppUserRepo appUserRepo)
        : base(userOrgsService)
        {
            _appUserRepo = appUserRepo;
        }

        // Currently this controller has no purpose
        public async Task<IActionResult> Index(CancellationToken ct)
        {

            var user = await _appUserRepo.GetAppUser(HttpContext.User) ?? throw new InvalidOperationException("Logged in user not found");

            var activeOrg = await GetActiveOrgSetName(user.Id, ct);

            if (activeOrg == null)
            {
                return RedirectToAction("Index", "Orgs", new { error = 1 });
            }

            return View();
        }
    }
}
