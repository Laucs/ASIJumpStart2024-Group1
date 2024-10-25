using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class CategoryController : ControllerBase<CategoryController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;

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
        public CategoryController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IExpenseService expenseService,
                            ICategoryService categoryService,
                         
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._categoryService = categoryService;
            this._expenseService = expenseService;
        }

        /// <summary>
        /// Summary Method
        /// </summary>
        /// <returns>Analytics Dashboard</returns>
        [HttpGet]
        public IActionResult Details()
        {
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = Convert.ToInt32(claimsUserId);

            var categories = _categoryService.RetrieveAll(userId);

            var viewModel = new CategoryPageViewModel
            {
                Categories = categories,
                NewCategory = new CategoryViewModel() // Initialize for any new category logic
            };

            ViewData["ActivePage"] = "Category";
            return View(viewModel);
        }



        [HttpPost]
        public IActionResult PostCategory(CategoryPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the logged-in user's ID
                var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = Convert.ToInt32(claimsUserId);

                // Assign the userId to the new category
                model.NewCategory.UserId = userId;

                // Call the service to add the new category
                _categoryService.Add(model);
                TempData["AddSuccess"] = "Category added successfully!";
                // Redirect after successful category creation
                return RedirectToAction("Details", "Category");
            }

            return View(model); // Return the view if model state is invalid
        }
        [HttpPost]
        public IActionResult DeleteExpense(int expenseId)
        {
            _expenseService.Delete(expenseId);
            return RedirectToAction("Details", "Category");

        }

    }

}

