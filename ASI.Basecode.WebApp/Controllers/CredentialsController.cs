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
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Models.LoginViewModel model, string returnUrl)
        {

            this._session.SetString("HasSession", "Exist");

            MUser user = new()
            {
                UserCode = model.UserCode,
                Password = model.Password,
            };

            // Authenticate user
            var loginResult = _userService.AuthenticateUser(model.UserCode, model.Password, ref user);

            if (loginResult == LoginResult.Success)
            {
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", string.Join(" ", user.FirstName, user.LastName));
                TempData["SuccessMessage"] = "Successfully Logged In";
                return RedirectToAction("Summary", "Analytics");
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect UserId or Password";
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
                Console.WriteLine("Starting Registration");

                // Check if username is taken
                if (_userService.IsUsernameTaken(model.Username))
                {
                    Console.WriteLine($"Username {model.Username} is already taken.");
                    ModelState.AddModelError("Username", "This username is already taken.");
                }

                // Check if email is taken
                if (_userService.IsEmailTaken(model.Mail))
                {
                    Console.WriteLine($"Email {model.Mail} is already registered.");
                    ModelState.AddModelError("Mail", "This email is already registered.");
                }

                // If no validation errors
                if (ModelState.IsValid)
                {
                    Console.WriteLine("Model is valid, creating user.");

                    // Create user from model
                    var user = CreateUserFromModel(model);
                    user.EmailVerificationToken = Guid.NewGuid().ToString(); // Generate token
                    user.VerificationTokenExpiration = DateTime.Now.AddMinutes(5); // Set expiration

                    // Log the generated token and expiration time
                    Console.WriteLine($"Generated Token: {user.EmailVerificationToken}");
                    Console.WriteLine($"Token Expiration: {user.VerificationTokenExpiration}");

                    // Add user to the database
                    _userService.Add(new UserViewModel
                    {
                        UserCode = model.Username,
                        Mail = model.Mail,
                        FirstName = null,
                        LastName = null,
                        Password = model.Password,
                        EmailVerificationToken = user.EmailVerificationToken, // Store token
                        VerificationTokenExpiration = user.VerificationTokenExpiration // Store expiration
                    });

                    // Log email sending attempt
                    Console.WriteLine($"Sending verification email to {model.Mail}.");

                    // Send verification email
                    await _emailService.SendEmailAsync(model.Mail, "Verify your email",
                        $"Please verify your email by clicking this link: " +
                        $"{Url.Action("VerifyEmail", "Credentials", new { token = user.EmailVerificationToken }, Request.Scheme)}");

                    // Log success
                    Console.WriteLine("Registration successful, verification email sent.");
                    TempData["SuccessMessage"] = "Successfully registered! Please check your email to verify your account.";

                    return RedirectToAction("Login", "Credentials");
                }
                else
                {
                    Console.WriteLine("Model validation failed.");
                }
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Invalid data exception: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                TempData["ErrorMessage"] = "Server Error";
            }

            Console.WriteLine("Returning to registration view due to error.");
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail(string token)
        {
            Console.WriteLine($"Verifying email with token: {token}");

            var user = _userService.GetUserByVerificationToken(token);

            if (user != null)
            {
                Console.WriteLine($"Token valid, verifying user with ID: {user.UserId}");

                // Mark email as verified
                user.IsEmailVerified = true;
                user.VerificationTokenExpiration = null;

                _userService.Update(new UserViewModel
                {
                    Id = user.UserId,
                    UserCode = user.UserCode,
                    IsEmailVerified = true
                });

                TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
                Console.WriteLine("Email verified successfully.");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired token.";
                Console.WriteLine("Invalid or expired token.");
            }

            return RedirectToAction("Login", "Credentials");
        }




    }

}