﻿using ASI.Basecode.Data.Models;
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
        public CategoryController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IExpenseService expenseService,
                            ICategoryService categoryService,
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
            this._categoryService = categoryService;
            this._expenseService = expenseService;
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
            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var expense = _expenseService.RetrieveExpense(expenseId);

                if (expense != null && expense.UserId == userId)
                {
                    // Restore the amount to the category budget
                    _walletService.UpdateBalanceAfterExpenseRemoval(userId, expense.Amount, expense.CategoryId);
                    _expenseService.Delete(expenseId);
                    
                    TempData["DeleteSuccess"] = "Expense deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Expense not found or unauthorized";
                }
                return RedirectToAction("Details", "Category");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the expense";
                return RedirectToAction("Details", "Category");
            }
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
        public IActionResult DeleteCategory(int categoryId, bool checkExpenses = true, bool confirmed = false)
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (checkExpenses)
                {
                    var hasExpenses = _categoryService.HasExpenses(categoryId);
                    if (hasExpenses)
                    {
                        TempData["ConfirmDelete"] = categoryId;
                        TempData["Message"] = "This category has expenses and a budget. Are you sure you want to delete everything?";
                        return RedirectToAction("Details");
                    }
                    else if (confirmed)
                    {
                        // Delete wallet instance first
                        _walletService.DeleteWalletForCategory(userId, categoryId);
                        _categoryService.Delete(categoryId);
                        TempData["AddSuccess"] = "Category deleted successfully.";
                        return RedirectToAction("Details");
                    }
                }

                // Handle confirmed deletion with expenses
                if (TempData["ConfirmDelete"] != null || !checkExpenses)
                {
                    // Delete all related expenses first
                    _expenseService.DeleteExpensesByCategoryId(categoryId);
                    
                    // Delete wallet instance
                    _walletService.DeleteWalletForCategory(userId, categoryId);
                    
                    // Finally delete the category
                    _categoryService.Delete(categoryId);
                    TempData["AddSuccess"] = "Category and all related data deleted successfully.";
                }

                return RedirectToAction("Details");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details");
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
                    var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    var existingExpense = _expenseService.RetrieveExpense(expenseDto.ExpenseId);

                    if (existingExpense == null)
                    {
                        TempData["ErrorMessage"] = "Expense not found.";
                        return RedirectToAction("Details", "Category");
                    }

                    if (existingExpense.UserId != userId)
                    {
                        TempData["ErrorMessage"] = "Unauthorized to edit this expense.";
                        return RedirectToAction("Details", "Category");
                    }

                    // Calculate the difference in amount
                    decimal amountDifference = expenseDto.Amount - existingExpense.Amount;

                    // If category changed, handle both old and new category budgets
                    if (existingExpense.CategoryId != expenseDto.CategoryId)
                    {
                        // Restore amount to old category
                        _walletService.UpdateBalanceAfterExpenseRemoval(userId, existingExpense.Amount, existingExpense.CategoryId);

                        // Check if new category has sufficient balance
                        var newCategoryBalance = _walletService.GetBalance(userId, expenseDto.CategoryId);
                        if (expenseDto.Amount > newCategoryBalance)
                        {
                            TempData["ErrorMessage"] = $"Insufficient budget in new category. Available: ₱{newCategoryBalance:N2}";
                            return RedirectToAction("Details", "Category");
                        }

                        // Deduct from new category
                        _walletService.DeductExpense(userId, expenseDto.Amount, expenseDto.CategoryId);
                    }
                    else if (amountDifference != 0) // Same category but amount changed
                    {
                        var categoryBalance = _walletService.GetBalance(userId, expenseDto.CategoryId);
                        if (amountDifference > 0 && amountDifference > categoryBalance)
                        {
                            TempData["ErrorMessage"] = $"Insufficient budget for increase. Available: ₱{categoryBalance:N2}";
                            return RedirectToAction("Details", "Category");
                        }

                        // Update the balance based on the difference
                        if (amountDifference > 0)
                        {
                            _walletService.DeductExpense(userId, amountDifference, expenseDto.CategoryId);
                        }
                        else
                        {
                            _walletService.UpdateBalanceAfterExpenseRemoval(userId, Math.Abs(amountDifference), expenseDto.CategoryId);
                        }
                    }

                    // Update expense properties
                    existingExpense.ExpenseName = expenseDto.ExpenseName;
                    existingExpense.Amount = expenseDto.Amount;
                    existingExpense.CategoryId = expenseDto.CategoryId;
                    existingExpense.CreatedDate = expenseDto.CreatedDate;
                    existingExpense.Description = expenseDto.Description;

                    _expenseService.Update(existingExpense);
                    TempData["AddSuccess"] = "Expense updated successfully!";
                    return RedirectToAction("Details", "Category");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                    return RedirectToAction("Details", "Category");
                }
            }
            return RedirectToAction("Details", "Category");
        }
    }

}

