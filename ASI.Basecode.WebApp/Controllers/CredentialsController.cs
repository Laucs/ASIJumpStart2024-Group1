using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class CredentialsController : ControllerBase<CredentialsController>
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
        public CredentialsController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                             IEmailService emailService,
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
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        /// 
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(SigninViewModel model, string returnUrl)
        {
            this._session.SetString("HasSession", "Exist");

            MUser user = new()
            {
                UserCode = model.UserCode,
                Password = model.Password,
            };

            var loginResult = _userService.AuthenticateUser(model.UserCode, model.Password, ref user);

            if (loginResult == LoginResult.Success)
            {
                await this._signInManager.SignInAsync(user);
                TempData["LoginSuccess"] = $"Successfully logged in as {user.UserCode}";
                return RedirectToAction("Summary", "Analytics");
            }
            else if (loginResult == LoginResult.EmailNotVerified)
            {
                TempData["EmailNotVerified"] = "Your email address is not verified. Please check your inbox for the verification link.";
                return View(); // Return to the login view or appropriate view
            }
            else
            {
                TempData["InvalidCred"] = "Incorrect Username or Password. Please Try Again!";
                return View();
            }

        }




        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                // Check if username is taken
                if (_userService.IsUsernameTaken(model.Username))
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                }

                // Check if email is taken
                if (_userService.IsEmailTaken(model.Mail))
                {
                    ModelState.AddModelError("Mail", "This email is already registered.");
                }

                // If no validation errors
                if (ModelState.IsValid)
                {

                    // Create user from model
                    var user = CreateUserFromModel(model);
                    user.EmailVerificationToken = Guid.NewGuid().ToString(); // Generate token
                    user.VerificationTokenExpiration = DateTime.Now.AddMinutes(5); // Set expiration

                    // Add user to the database
                    _userService.Add(new UserViewModel
                    {
                        UserCode = model.Username,
                        Mail = model.Mail,
                        FirstName = null,
                        LastName = null,
                        Password = model.Password,
                        EmailVerificationToken = user.EmailVerificationToken,
                        VerificationTokenExpiration = user.VerificationTokenExpiration
                    });


                    // Send verification email
                    await SendVerificationEmail(model.Mail, user.EmailVerificationToken);
                    TempData["RegSuccess"] = "Successfully registered! Please check your email to verify your account.";

                    return RedirectToAction("Login", "Credentials");
                }
                else
                {
                    Console.WriteLine("Model validation failed.");
                }
            }
            catch (InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Server Error";
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

       
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Re-render form if model validation fails
            }

            // Check if the email exists
            var user = _userService.GetUserByEmail(model.Email);

            if (user != null)
            {
                // Generate reset token and expiration
                var resetToken = Guid.NewGuid().ToString();
                user.PasswordResetToken = resetToken;
                user.ResetTokenExpiration = DateTime.Now.AddHours(1); // Token valid for 1 hour

                // Update user details with reset token
                _userService.Update(new UserViewModel
                {
                    Id = user.UserId,
                    UserCode = user.UserCode,
                    PasswordResetToken = resetToken,
                    ResetTokenExpiration = user.ResetTokenExpiration
                });

                // Send reset password email
                await SendResetPasswordEmail(model.Email, resetToken);

                TempData["SendPassToken"] = "Please check your email. A password reset link has been sent.";
            }
            else
            {
                // Inform user that email is not registered
                TempData["EmailNotFound"] = "The email address is not associated with any account.";
                return View(model);

            }

            return RedirectToAction("Login", "Credentials");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["InvalidResetToken"] = "Invalid or expired password reset token.";
                return RedirectToAction("RPTokenExpired");
            }

            // Check if the token exists and is valid
            var user = _userService.GetUserByPasswordResetToken(token);
            if (user == null || user.ResetTokenExpiration < DateTime.Now)
            {
                TempData["InvalidResetToken"] = "Invalid or expired password reset token.";
                return RedirectToAction("RPTokenExpired");
            }

            // Initialize the ResetPasswordViewModel with the token
            var model = new ResetPasswordViewModel
            {
                Token = token
            };

            return View(model); // Pass the model with the token to the view
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the user by the token
                var user = _userService.GetUserByPasswordResetToken(model.Token);
                if (user == null || user.ResetTokenExpiration < DateTime.Now)
                {
                    // If token is invalid or expired, redirect to TokenExpired view
                    return RedirectToAction("RPTokenExpired");
                }

                // Update user's password
                user.Password = PasswordManager.EncryptPassword(model.NewPassword);
                user.PasswordResetToken = null; // Clear the reset token
                user.ResetTokenExpiration = null; // Clear the expiration date

                // Update user information in the database
                _userService.Update(new UserViewModel
                {
                    Id = user.UserId,
                    Password = user.Password,
                    UserCode = user.UserCode
                });

                TempData["ResetSuccess"] = "Password successfully reset. You can now log in with your new password.";
                return RedirectToAction("Login", "Credentials");
            }

            return View(model); // Return the view with validation errors if model is not valid
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {


            if (changePasswordViewModel == null)
            {
                return View("Error");
            }
            return View();
        }


        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this._signInManager.SignOutAsync();

            // Set a success message in TempData
            TempData["LogoutSuccess"] = "You have logged out successfully.";

            return RedirectToAction("Login", "Credentials");
        }


        // Method to create a new user
        private MUser CreateUserFromModel(RegisterViewModel model)
        {
            return new MUser
            {
                Mail = model.Mail,
                UserCode = model.Username,
                Password = PasswordManager.EncryptPassword(model.Password),
                FirstName = null,
                LastName = null,
                InsBy = null,
                InsDt = DateTime.Now,
                UpdBy = null,
                UpdDt = DateTime.Now,
                Deleted = false,
                Remarks = "USER",
                TemporaryPassword = null
            };
        }

        private async Task SendVerificationEmail(string email, string token)
        {
            var verificationLink = Url.Action("VerifyEmail", "Credentials", new { token = token }, Request.Scheme);
            var emailBody = $"Please verify your email by clicking <a href='{verificationLink}'>here</a>. You have 5 minutes to verify your account.";

            await _emailService.SendEmailAsync(email, "EXTR - Email Verification", emailBody);
        }

        private async Task SendResetPasswordEmail(string email, string resetToken)
        {
            // Generate reset link
            var resetLink = Url.Action("ResetPassword", "Credentials", new { token = resetToken }, Request.Scheme);

            // Send reset email
            var emailBody = $"Please reset your password by clicking <a href='{resetLink}'>here</a>. If you did not make this request, please do not click this link.";
            await _emailService.SendEmailAsync(email, "EXTR - Password Reset Verification", emailBody);
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail(string token)
        {
            var user = _userService.GetUserByVerificationToken(token);

            if (user != null)
            {
                // Mark email as verified
                user.IsEmailVerified = true;
                user.VerificationTokenExpiration = null;

                _userService.Update(new UserViewModel
                {
                    Id = user.UserId,
                    UserCode = user.UserCode,
                    IsEmailVerified = true
                });
                TempData["EmailVerified"] = "Email verified successfully! You can login.";
                return RedirectToAction("Login", "Credentials");

            }
            else
            {
                TempData["TokenExpired"] = "Invalid or expired token.";
                return RedirectToAction("RegTokenExpired", "Credentials");

            }

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegTokenExpired() { 
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RPTokenExpired()
        {
            return View();
        }

    }

}