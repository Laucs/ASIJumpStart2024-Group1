using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
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
    public class ExpenseController : ControllerBase<ExpenseController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IExpenseService _expenseService;
        private readonly ICategoryService _categoryService;
        private readonly IWalletService _walletService;

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
        public ExpenseController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            ICategoryService categoryService,
                            IExpenseService expenseService,
                            IWalletService walletService,
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
            this._walletService = walletService;
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

            var userCode = HttpContext.User.FindFirst("UserCode")?.Value;
            var profilePic = _userService.GetUserProfilePic(userCode);
            ViewBag.ProfilePic = profilePic;

            var categories = _categoryService.RetrieveAll(userId: userId);

            var model = new ExpenseViewModel
            {
                Categories = categories.ToList()
            };

            ViewData["ActivePage"] = "Expense";
            return View(model);
        }


        [HttpPost]
        public IActionResult PostExpense(ExpenseViewModel model)
        {
            var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (ModelState.IsValid)
            {
                try
                {
                    // Get category-specific balance
                    var categoryBalance = _walletService.GetBalance(userId, model.CategoryId);

                    // Check if category has sufficient balance
                    if (model.Amount > categoryBalance)
                    {
                        TempData["ErrorMessage"] = $"Insufficient budget for {_categoryService.GetById(model.CategoryId).CategoryTitle}. Current budget: ₱{categoryBalance:N2}";
                        model.Categories = _categoryService.RetrieveAll(userId: userId).ToList();
                        return View("Details", model);
                    }

                    model.UserId = userId;
                    _expenseService.Add(model);

                    // Deduct only from category budget
                    _walletService.DeductExpense(userId, model.Amount, model.CategoryId);

                    TempData["SuccessMessage"] = "Expense added successfully!";
                    return RedirectToAction("Details", "Expense");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding expense");
                    TempData["ErrorMessage"] = "An error occurred while adding the expense.";
                    model.Categories = _categoryService.RetrieveAll(userId: userId).ToList();
                    return View("Details", model);
                }
            }

            model.Categories = _categoryService.RetrieveAll(userId: userId).ToList();
            return View("Details", model);
        }

        [HttpPost]
        public IActionResult DeleteExpense(int expenseId)
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var expense = _expenseService.GetExpenseById(expenseId);

                if (expense != null && expense.UserId == userId)
                {
                    // Restore the amount to the category budget
                    _walletService.UpdateBalanceAfterExpenseRemoval(userId, expense.Amount, expense.CategoryId);

                    _expenseService.Delete(expenseId);
                    return Json(new { success = true, message = "Expense deleted successfully" });
                }

                return Json(new { success = false, message = "Expense not found or unauthorized" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense");
                return Json(new { success = false, message = "An error occurred while deleting the expense" });
            }
        }
    }
}
