using Microsoft.AspNetCore.Mvc;
using VontaGenta.Models;
using VontaGenta.Services;
using VontaGenta.ViewHelpers;
using VontaGenta.Entities;

namespace VontaGenta.Controllers
{
    public class BaseController : Controller
    {

        protected readonly IUserOrgsService? _userOrgsService;

        protected BaseController(IUserOrgsService? userOrgsService = null)
        {
            _userOrgsService = userOrgsService;
        }

        protected void SetStatus(string message)
        {
            TempData["StatusMessage"] = message;
        }

        protected string? GetStatus()
        {
            return (string?)TempData["StatusMessage"];
        }

        protected bool HasStatusMessage()
        {
            return (string?)TempData.Peek("StatusMessage") != null;
        }

        protected void SetError(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected string? GetError()
        {
            return (string?)TempData["ErrorMessage"];
        }

        protected bool HasErrorMessage()
        {
            return (string?)TempData.Peek("ErrorMessage") != null;
        }

        protected float GetSystemVersion(IConfiguration _config)
        {
            float sysVerFloat = 0f;
            var sysVer = _config["SystemVersion"];

            if (string.IsNullOrEmpty(sysVer))
            {
                return sysVerFloat;
            }

            try
            {
                sysVerFloat = float.Parse(sysVer);
            }
            catch {
                // TODO log error
            }

            return sysVerFloat;
        }

        protected async Task<Org?> GetActiveOrgSetName(string userId, CancellationToken ct)
        {
            if (_userOrgsService == null)
            {
                return null;
            }

            var activeOrgDto = await _userOrgsService.GetActiveOrg(userId, ct);

            if (activeOrgDto.IsSuccess)
            {
                ViewData["OrgName"] = activeOrgDto.Org.Name;
                return activeOrgDto.Org;
            }

            return null;
        }

        #region Select List Helpers

        public bool SetCategoriesSelectList(IHasCategoriesSelectLists vm)
        {
            var selectListHelpers = new SelectListHelpers();

            var error = selectListHelpers.SetCategoriesSelectList(vm);

            if (!string.IsNullOrEmpty(error))
            {
                SetError(error);
                return false;
            }

            return true;
        }

        public bool SetProjectSelectList(IHasProjectsSelectLists vm)
        {
            var selectListHelpers = new SelectListHelpers();

            var error = selectListHelpers.SetProjectSelectList(vm);

            if (!string.IsNullOrEmpty(error))
            {
                SetError(error);
                return false;
            }

            return true;
        }

        public bool SetCurrencySelectList(IHasCurrenciesSelectLists vm)
        {
            var selectListHelpers = new SelectListHelpers();

            var error = selectListHelpers.SetCurrenciesSelectList(vm);

            if (!string.IsNullOrEmpty(error))
            {
                SetError(error);
                return false;
            }

            return true;
        }

        public bool SetClientSelectList(IHasClientsSelectLists vm)
        {
            var selectListHelpers = new SelectListHelpers();

            var error = selectListHelpers.SetClientSelectList(vm);

            if (!string.IsNullOrEmpty(error))
            {
                SetError(error);
                return false;
            }

            return true;
        }

        #endregion

    }
}
