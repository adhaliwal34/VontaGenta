// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VontaGenta.Entities;
using VontaGenta.Data;
using System.ComponentModel.DataAnnotations;

namespace VontaGenta.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAppTimeZonesRepo _appTimeZonesRepo;

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAppTimeZonesRepo appTimeZonesRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appTimeZonesRepo = appTimeZonesRepo;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First names")]
            public string FirstNames { get; set; }

            [Required]
            [Display(Name = "Last name")]
            [DataType(DataType.Text)]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Time zone")]
            [DataType(DataType.Text)]
            public int TimeZoneId { get; set; }

            public SelectList TimeZonesSelectList { get; set; }
        }

        private async Task LoadAsync(AppUser user, CancellationToken ct)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstNames = user.FirstNames,
                LastName = user.LastName,
                TimeZoneId = user.TimeZoneId
            };

            var timeZones = await _appTimeZonesRepo.GetTimeZones(ct);
            Input.TimeZonesSelectList = new SelectList(timeZones, "ID", "FriendlyName");
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user, ct);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user, ct);
                return Page();
            }

            if (Input.FirstNames != user.FirstNames)
            {
                user.FirstNames = Input.FirstNames;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }

            if (Input.TimeZoneId != user.TimeZoneId)
            {
                user.TimeZoneId = Input.TimeZoneId;
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
