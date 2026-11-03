// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using VontaGenta.Entities;
using VontaGenta.Data;
using VontaGenta.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VontaGenta.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IHcaptchaService _hcaptchaService;
        // Accessing the repo directly. Not using a service. Haven't created a service layer for this.
        private readonly IAppTimeZonesRepo _appTimeZonesRepo;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailService emailService,
            IConfiguration config,
            IHcaptchaService hcaptchaService,
            IAppTimeZonesRepo appTimeZonesRepo
            )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _config = config;
            _hcaptchaService = hcaptchaService;
            _appTimeZonesRepo = appTimeZonesRepo;
        }

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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string HcaptchaError { get; set; }

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
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First names")]
            [RegularExpression(@"^[a-zA-Z]+ [a-zA-Z]+$|^[a-zA-Z]+$", ErrorMessage = "The {0} can only contain letters a to z and A to Z and one space between names.")]
            [StringLength(30, ErrorMessage = "The maximum length is 30 characters")]
            public string FirstNames { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Last name")]            
            [RegularExpression(@"^[a-zA-Z]+[\-'][a-zA-Z]+$|^[a-zA-Z]+$", ErrorMessage = "The {0} can only contain letters a to z and A to Z one dash or single quote is allowed between names.")]
            [StringLength(40, ErrorMessage = "The maximum length is 40 characters")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Time zone")]
            [DataType(DataType.Text)]
            public int TimeZoneId { get; set; }

            public SelectList TimeZonesSelectList { get; set; }
        }


        public async Task<IActionResult> OnGetAsync(CancellationToken ct, string returnUrl = null)
        {
            if (!_config["RegistrationEnabled"].ToLower().Equals("true"))
            {
                return RedirectToAction("Index", "Home");
            }
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Input = new InputModel();

            var timeZones = await _appTimeZonesRepo.GetTimeZones(ct);
            Input.TimeZonesSelectList = new SelectList(timeZones, "ID", "FriendlyName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!_config["RegistrationEnabled"].ToLower().Equals("true"))
            {
                return RedirectToAction("Index", "Home");
            }

            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstNames = Input.FirstNames;
                user.LastName = Input.LastName;
                user.TimeZoneId = Input.TimeZoneId;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var hcaptchaResponse = Request.Form["h-captcha-response"];
                
                var serviceResponse = await _hcaptchaService.VerifyResponse(hcaptchaResponse);

                if (!serviceResponse.IsSuccess)
                {
                    HcaptchaError = "Error validating hCaptcha, please try again.";
                    return Page();
                }
                
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    _emailService.SendMail(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == nameof(IdentityErrorDescriber.DuplicateUserName))
                    {
                        _emailService.SendMail(Input.Email, "Attempt to register your email",
                            "An attempt was made to register your email on the site, but it is already registered. If you have forgotten your password please reset it on the Login page.");
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
