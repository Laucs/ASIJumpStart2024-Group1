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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AnalyticsController : ControllerBase<AnalyticsController>
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
        public AnalyticsController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            ICategoryService categoryService,
                            IExpenseService expenseService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._expenseService = expenseService;
            this._categoryService = categoryService;
        }

        /// <summary>
        /// Summary Method
        /// </summary>
        /// <returns>Analytics Dashboard</returns>
        [HttpGet]
        public IActionResult Summary()
        {
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = Convert.ToInt32(claimsUserId);

            var expenses = _expenseService.RetrieveAll(userId: userId);
            var categories = _categoryService.RetrieveAll(userId: userId);

            var groupedCategories = expenses
                .Join(categories,
                    expense => expense.CategoryId,
                    category => category.CategoryId,
                    (expense, category) => new { expense, category })
                .GroupBy(ec => new { ec.category.CategoryId, ec.category.CategoryTitle })
                .Select(g => new CategoryViewModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryTitle = g.Key.CategoryTitle,
                    TotalAmount = g.Sum(ec => ec.expense.Amount)
                })
                .ToList();

            var model = new SummaryViewModel
            {
                CategoryAnalytics = groupedCategories,
                ExpenseAnalytics = expenses.Select(e => new ExpenseViewModel { }).ToList(),
                TotalExpenses = expenses.Sum(e => e.Amount)
            };

            ViewData["ActivePage"] = "Analytics";
            return View(model);
        }
    }
}
