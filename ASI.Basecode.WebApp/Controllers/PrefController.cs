using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;
using System;
using System.Data;
using System.IO;
using System.Security.Claims;
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
            var user = _userService.RetrieveUserByUsername(currentUsername);

            var userData = new ProfileViewModel()
            {
                UserCode = user.UserCode ?? "",  // Default to empty string if UserCode is null
                Email = user.Mail ?? "",  // Default to empty string if Mail is null
                UpdateEmail = new ChangeEmailViewModel(),
                UpdatePassword = new ChangePasswordViewModel(),
                IsPasswordChangeRequired = false,
                ProfilePicture = user.ProfilePic // Assuming ProfilePic can be null
            };
            ViewData["ActivePage"] = "Settings";
            return View(userData);
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

                var user = _userService.RetrieveUserByUsername(currentUsername);
                user.ProfilePic = $"/img/profile/{fileName}";

                var userData = new MUser()
                {
                    UserCode = user.UserCode,
                    UserId = user.Id
                };
                _signInManager.CreateClaimsIdentity(userData);
                _userService.UpdateProfile(user);


                return Json(new { filePath = user.ProfilePic });
            }

            return BadRequest(new { message = "File upload failed." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(ProfileViewModel model)
        {
            // Retrieve the current user's username from claims
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;

            // Get the user data by the current username
            var userData = _userService.RetrieveUserByUsername(currentUsername);

            // Check if the new username is already taken by another user
            if (_userService.UsernameAvailability(model.UpdateUsername.NewUsername, userData.Id))
            {
                ModelState.AddModelError("UpdateEmailOrUserName.NewUsername", "This username is already taken.");
                return View("Settings"); // Return to the settings view with errors
            }

            // Check if the new username is different from the current one
            if (!model.UpdateUsername.NewUsername.Equals(userData.UserCode, StringComparison.OrdinalIgnoreCase))
            {
                // Update the username
                userData.UserCode = model.UpdateUsername.NewUsername;
                _userService.UpdateUsername(userData);

                // Retrieve the updated user data
                var updatedUser = _userService.RetrieveUserByUsername(userData.UserCode);

                // Prepare user data for re-authentication
                var data = new MUser
                {
                    UserId = updatedUser.Id,
                    UserCode = updatedUser.UserCode,
                    Password = updatedUser.Password // Assuming password is hashed
                };

                // Notify the user of the successful change
                TempData["ChangeSuccess"] = "Username updated successfully!";

                // Re-sign the user in with updated claims
                await _signInManager.SignInAsync(data);

                // Redirect to prevent resubmission
                return RedirectToAction("Settings");
            }

            // If no change, notify user and return to the view
            TempData["InfoMessage"] = "No changes were made to the username.";
            return RedirectToAction("Settings");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateEmail(ProfileViewModel model)
        {
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;

            var userData = _userService.RetrieveUserByUsername(currentUsername);

            if (_userService.EmailAvailability(model.UpdateEmail.NewEmail, userData.Id))
            {
                ModelState.AddModelError("UpdateEmail.NewEmail", "This email is already registered.");
                return View("Settings", PopulateData(userData));
            }

            if (!model.UpdateEmail.NewEmail.Equals(userData.Mail))
            {
                userData.Mail = model.UpdateEmail.NewEmail;

                userData.EmailVerificationToken = Guid.NewGuid().ToString();
                userData.VerificationTokenExpiration = DateTime.Now.AddMinutes(5);

                _userService.UpdateEmail(userData);

                await SendVerificationEmail(userData.Mail, userData.EmailVerificationToken);
                TempData["RegSuccess"] = "Email changed successfully! Please verify your new email.";

                return RedirectToAction("Login", "Credentials");
            }

            TempData["InfoMessage"] = "No changes were made to the email.";
            return View("Settings", PopulateData(userData));
        }

        [HttpPost]
        public IActionResult UpdatePassword(ProfileViewModel model)
        {
            var currentUsername = HttpContext.User.FindFirst("UserCode")?.Value;
            var userData = _userService.RetrieveUserByUsername(currentUsername);


            // Check if the old password matches the current password
            if (!model.UpdatePassword.OldPassword.Equals(userData.Password))
            {
                ModelState.AddModelError("UpdatePassword.OldPassword", "The old password does not match.");
                return View("Settings", model: PopulateData(userData));
            }

            // Proceed with the password change if validation is successful
            if (!string.IsNullOrWhiteSpace(model.UpdatePassword.NewPassword) && ModelState.IsValid)
            {
                userData.Password = model.UpdatePassword.NewPassword;
                // Update password in the database
                _userService.UpdatePassword(userData);
                TempData["ChangeSuccess"] = "Password updated successfully!";
                return RedirectToAction("Settings", "Pref");
            }
            // If ModelState is not valid, then displays all validations
            return View("Settings", model: PopulateData(userData));
        }
                       
      
        private async Task SendVerificationEmail(string email, string token)
        {
            var verificationLink = Url.Action("VerifyEmail", "Credentials", new { token = token }, Request.Scheme);
            var emailBody = $"Please verify your email by clicking <a href='{verificationLink}'>here</a>. You have 5 minutes to verify your account.";

            await _emailService.SendEmailAsync(email, "EXTR - Email Verification", emailBody);
        }
        public ProfileViewModel PopulateData(UserViewModel userData)
        {
            return new ProfileViewModel()
            {
                UserCode = userData.UserCode ?? "",  // Default to empty string if UserCode is null
                Email = userData.Mail ?? "",  // Default to empty string if Mail is null
                UpdateEmail = new ChangeEmailViewModel(),
                UpdatePassword = new ChangePasswordViewModel(),
                IsPasswordChangeRequired = false,
                ProfilePicture = userData.ProfilePic // Assuming ProfilePic can be null
            };
        }
    }
}
