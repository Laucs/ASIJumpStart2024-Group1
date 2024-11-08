using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class PrefController : ControllerBase<PrefController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public PrefController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IEmailService emailService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._emailService = emailService;
        }

        /// <summary>
        /// Summary Method
        /// </summary>
        /// <returns>Analytics Dashboard</returns>
        [HttpGet]
        public IActionResult Settings()
        {
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;

            var data = _userService.RetrieveUserByUsername(currentUsername);
            var model = new ProfileViewModel()
            {
                UserCode = data.UserCode,
                Email = data.Mail,
                UpdateEmailOrUserName = new ChangeEmailOrUsernameViewModel(),
                UpdatePassword = new ChangePasswordViewModel(),
                ProfilePicture = data.ProfilePic
            };



            ViewData["ActivePage"] = "Settings";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveImagePath(IFormFile ProfilePicture)
        {
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "profile");
                var fileName = Path.GetFileName(ProfilePicture.FileName);
                var fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                var model = new ProfileViewModel
                {
                    UserCode = currentUsername,
                    ProfilePicture = $"/img/profile/{fileName}"
                };


                TempData["ChangeSuccess"] = "Profile Picture changed successfully!";

                _userService.UpdateProfile(model);



                return Json(new { filePath = model.ProfilePicture });
            }

            return BadRequest(new { message = "File upload failed." });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;
            var userData = _userService.RetrieveUserByUsername(currentUsername);

            // Email availability check
            if (_userService.EmailAvailability(model.UpdateEmailOrUserName.NewEmail, userData.Id))
            {
                ModelState.AddModelError("UpdateEmailOrUserName.NewEmail", "This email is already registered.");
            }

            // Username availability check
            if (_userService.UsernameAvailability(model.UpdateEmailOrUserName.NewUsername, userData.Id))
            {
                ModelState.AddModelError("UpdateEmailOrUserName.NewUsername", "This username is already taken.");
            }

            // Old password match check
            if (model.IsPasswordChangeRequired)
            {
                if (model.IsPasswordChangeRequired && !model.UpdatePassword.OldPassword.Equals(userData.Password))
                {
                    ModelState.AddModelError("UpdatePassword.OldPassword", "The old password does not match.");
                }

                if (model.UpdatePassword.NewPassword.Equals(model.UpdatePassword.OldPassword))
                {
                    ModelState.AddModelError("UpdatePassword.NewPassword", "The new password should be different from the old password.");
                }
            }

            // If ModelState is invalid, return the view with error messages
            if (!ModelState.IsValid)
            {
                return View("Settings", model);
            }

            // Update password if it has changed
            if (model.IsPasswordChangeRequired && !string.IsNullOrWhiteSpace(model.UpdatePassword.NewPassword))
            {
                userData.Password = model.UpdatePassword.NewPassword;
            }

            // Update username if it has changed
            if (!model.UpdateEmailOrUserName.NewUsername.Equals(userData.UserCode, StringComparison.OrdinalIgnoreCase))
            {
                userData.UserCode = model.UpdateEmailOrUserName.NewUsername;
                var updatedUser = new MUser
                {
                    UserCode = userData.UserCode,
                    Password = userData.Password,
                };

                await _signInManager.SignInAsync(updatedUser);
            }

            // Update email if it has changed and send verification email
            if (!model.UpdateEmailOrUserName.NewEmail.Equals(userData.Mail, StringComparison.OrdinalIgnoreCase))
            {
                userData.Mail = model.UpdateEmailOrUserName.NewEmail.Trim();
                userData.EmailVerificationToken = Guid.NewGuid().ToString();
                userData.VerificationTokenExpiration = DateTime.Now.AddMinutes(5);

                _userService.Update(userData);
                await SendVerificationEmail(userData.Mail, userData.EmailVerificationToken);

                TempData["ChangeEmailSuccess"] = "Email changed successfully! Please verify your new email.";
                return RedirectToAction("Login", "Credentials");
            }

            // Update user information and display success message
            _userService.Update(userData);
            TempData["ChangeSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Settings");
        }

      
        private async Task SendVerificationEmail(string email, string token)
        {
            var verificationLink = Url.Action("VerifyEmail", "Credentials", new { token = token }, Request.Scheme);
            var emailBody = $"Please verify your email by clicking <a href='{verificationLink}'>here</a>. You have 5 minutes to verify your account.";

            await _emailService.SendEmailAsync(email, "EXTR - Email Verification", emailBody);
        }
    }
}
