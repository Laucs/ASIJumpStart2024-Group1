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
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var profilePic = _userService.GetUserProfilePic(userId);
            ViewBag.ProfilePic = profilePic;

            var categories = _categoryService.RetrieveAll(userId);

            var viewModel = new CategoryPageViewModel
            {
                Categories = categories,
                NewCategory = new CategoryViewModel()
            };

            ViewData["ActivePage"] = "Category";
            return View(viewModel);
        }



        [HttpPost]
        public IActionResult PostCategory(CategoryPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
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
                catch (InvalidOperationException ex)
                {
                    // Catch duplicate category error and set error message
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("Details", "Category");
                }
                catch (Exception ex)
                {
                    // Handle any other unexpected exceptions
                    TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                    return RedirectToAction("Details", "Category");
                }
            }

            // Return the view if model state is invalid
            return View(model);
        }
       
        [HttpPost]
        public IActionResult DeleteExpense(int expenseId)
        {
            _expenseService.Delete(expenseId);
            TempData["DeleteSuccess"] = "Expense deleted successfully!";
            return RedirectToAction("Details", "Category");

        }


        [HttpPost]
        public IActionResult EditCategory(CategoryPageViewModel categoryDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int userId = Convert.ToInt32(claimsUserId);

                    categoryDto.NewCategory.UserId = userId;

                    _categoryService.Update(categoryDto);
                    TempData["AddSuccess"] = "Category updated successfully!";
                    return RedirectToAction("Details");
                }
                catch (InvalidOperationException ex)
                {
                    // Add custom error message to ModelState if duplicate category
                    ModelState.AddModelError("", ex.Message);
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("Details");
                }
                catch (Exception ex)
                {
                    // General exception message
                    ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                    TempData["ErrorMessage"] = ex.Message;
                    return View("Details", categoryDto);
                }
            }

            return View("Details", categoryDto);
        }


        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            try
            {
                _categoryService.Delete(categoryId);
                return RedirectToAction("Details");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Details");
            }
        }

        //edit Expense
        [HttpPost]
        public IActionResult EditExpense(ExpenseViewModel expenseDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the current logged-in user's ID
                    var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int userId = Convert.ToInt32(claimsUserId);

                    // Fetch the existing expense based on the provided ExpenseId
                    var existingExpense = _expenseService.RetrieveExpense(expenseDto.ExpenseId);

                    // Check if the expense exists and is owned by the current user
                    if (existingExpense == null)
                    {
                        TempData["ErrorMessage"] = "Expense not found.";
                        _logger.LogError($"Expense with ID {expenseDto.ExpenseId} not found for user ID {userId}.");
                        return RedirectToAction("Details", "Category");
                    }

                    if (existingExpense.UserId != userId)
                    {
                        TempData["ErrorMessage"] = "Unauthorized to edit this expense.";
                        _logger.LogError($"Unauthorized access attempt: Expense UserId {existingExpense.UserId} vs. Current UserId {userId}");
                        return RedirectToAction("Details", "Category");
                    }

                    // Update expense properties with the new values from the form
                    existingExpense.ExpenseName = expenseDto.ExpenseName;
                    existingExpense.Amount = expenseDto.Amount;
                    existingExpense.CategoryId = expenseDto.CategoryId;
                    existingExpense.CreatedDate = expenseDto.CreatedDate;
                    existingExpense.Description = expenseDto.Description;

                    // Update the expense in the database
                    _expenseService.Update(existingExpense);
                    TempData["AddSuccess"] = "Expense updated successfully!";
                    _logger.LogInformation($"Expense ID {expenseDto.ExpenseId} updated successfully by user ID {userId}.");

                    return RedirectToAction("Details", "Category");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                    _logger.LogError($"Error updating expense ID {expenseDto.ExpenseId}: {ex.Message}");
                    return RedirectToAction("Details", "Category");
                }
            }
            else
            {
                // Capture model state errors to help with debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Invalid data submitted: " + string.Join("; ", errors);
                _logger.LogError("Model validation failed with errors: " + string.Join("; ", errors));

                return RedirectToAction("Details", "Category");
            }
        }









    }

}

