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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// <param name="walletService">The wallet service.</param>
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
                            TokenProviderOptionsFactory tokenProviderOptionsFactory,
                            IWalletService walletService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
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
        public IActionResult Summary(string filter = "all", string categoryFilter = "all")
        {
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = Convert.ToInt32(claimsUserId);

            var userCode = HttpContext.User.FindFirst("UserCode")?.Value;
            var profilePic = _userService.GetUserProfilePic(userCode);
            ViewBag.ProfilePic = profilePic;

            var expenses = _expenseService.RetrieveAll(userId: userId);
            var categories = _categoryService.RetrieveAll(userId: userId);

            DateTime? startDate = null;
            DateTime? endDate = null;

            switch (filter.ToLower())
            {
                case "today":
                    startDate = DateTime.Today;
                    endDate = DateTime.Today.AddDays(1).AddTicks(-1);
                    break;
                case "thisweek":
                    startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    endDate = startDate.Value.AddDays(7).AddTicks(-1);
                    break;
                case "thismonth":
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    endDate = startDate.Value.AddMonths(1).AddTicks(-1);
                    break;
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                expenses = expenses.Where(e => e.CreatedDate >= startDate && e.CreatedDate <= endDate).ToList();
            }

            if (categoryFilter.ToLower() != "all")
            {
                expenses = expenses.Where(e => categories.Any(c => c.CategoryTitle.ToLower() == categoryFilter.ToLower() && c.CategoryId == e.CategoryId)).ToList();
            }

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

            foreach (var category in categories)
            {
                category.MExpenses = expenses.Where(e => e.CategoryId == category.CategoryId)
                                              .Select(e => new MExpense
                                              {
                                                  ExpenseId = e.ExpenseId,
                                                  Amount = e.Amount,
                                                  DateCreated = e.CreatedDate,
                                                  ExpenseDescription = e.Description,
                                                  CategoryId = e.CategoryId,
                                                  UserId = e.UserId,
                                                  ExpenseName = e.ExpenseName
                                              }).ToList();
            }

            var categorizedExpenses = new CategoryPageViewModel
            {
                Categories = categories.Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryTitle = c.CategoryTitle,
                    MExpenses = c.MExpenses,
                }),
                NewCategory = new CategoryViewModel()
            };

            var model = new SummaryViewModel
            {
                SummaryAnalytics = groupedCategories,
                CategoryAnalytics = categorizedExpenses,
                ExpenseAnalytics = expenses.Select(e => new ExpenseViewModel { }).ToList(),
                TotalExpenses = expenses.Sum(e => e.Amount)
            };

            model.CurrentBalance = _walletService.GetBalance(userId);
            model.TotalExpenseAmount = expenses.Sum(e => e.Amount);
            model.RemainingBalance = model.CurrentBalance - model.TotalExpenseAmount;

            ViewData["ActivePage"] = "Analytics";
            return View(model);
        }

        [HttpGet("api/chart-data")]
        public IActionResult GetChartData()
        {
            var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = Convert.ToInt32(claimsUserId);

            var expenses = _expenseService.RetrieveAll(userId: userId);
            var categories = _categoryService.RetrieveAll(userId: userId);

            var weeklyData = GetWeeklyCategoryData(expenses);
            var (labels, datasets) = PrepareChartData(weeklyData, categories);

            if (expenses == null || !expenses.Any())
            {
                return Json(new { labels = new List<string>(), datasets = new List<ChartDataset>() });
            }

            if (categories == null || !categories.Any())
            {
                return Json(new { labels = new List<string>(), datasets = new List<ChartDataset>() });
            }

            return Json(new { labels, datasets });
        }

        private List<WeeklyCategoryData> GetWeeklyCategoryData(IEnumerable<ExpenseViewModel> expenses)
        {
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstDay;
            var firstDayOfWeek = DayOfWeek.Sunday;

            var weeklyData = expenses
                .GroupBy(e => new
                {
                    e.CategoryId,
                    Year = e.CreatedDate.Year,
                    Week = calendar.GetWeekOfYear(e.CreatedDate, weekRule, firstDayOfWeek)
                })
                .Select(g => new WeeklyCategoryData
                {
                    CategoryId = g.Key.CategoryId,
                    Year = g.Key.Year,
                    Week = g.Key.Week,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .ToList();

            // Debugging: Log the weekly data
            Console.WriteLine("Weekly Data: " + JsonConvert.SerializeObject(weeklyData));

            return weeklyData;
        }

        public class WeeklyCategoryData
        {
            public int CategoryId { get; set; }
            public int Year { get; set; }
            public int Week { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private (List<string> labels, List<ChartDataset> datasets) PrepareChartData(List<WeeklyCategoryData> weeklyData, IEnumerable<CategoryViewModel> categories)
        {
            var labels = new List<string>();
            var data = new Dictionary<int, List<decimal>>();

            // Initialize dictionary for each category
            foreach (var category in categories)
            {
                data[category.CategoryId] = new List<decimal>();
            }

            // Fill the data structure
            foreach (var weekData in weeklyData)
            {
                var label = $"Week {weekData.Week} of {weekData.Year}";
                if (!labels.Contains(label))
                {
                    labels.Add(label);
                }

                // Ensure the index corresponds to the correct week
                int weekIndex = labels.IndexOf(label);
                while (data[weekData.CategoryId].Count <= weekIndex)
                {
                    data[weekData.CategoryId].Add(0); // Fill in missing weeks with 0
                }
                data[weekData.CategoryId][weekIndex] = weekData.TotalAmount;
            }

            // Prepare datasets
            var datasets = new List<ChartDataset>();
            foreach (var category in categories)
            {
                datasets.Add(new ChartDataset
                {
                    label = category.CategoryTitle,
                    data = data[category.CategoryId],
                    borderColor = GetRandomColor(),
                    backgroundColor = "rgba(75, 192, 192, 0.2)",
                    fill = false,
                    tension = 0.1
                });
            }

            return (labels, datasets);
        }

        private string GetRandomColor()
        {
            Random random = new Random();
            int r = random.Next(256);
            int g = random.Next(256);
            int b = random.Next(256);
            return $"rgb({r}, {g}, {b})";
        }

        public class ChartDataset
        {
            public string label { get; set; }
            public List<decimal> data { get; set; }
            public string borderColor { get; set; }
            public string backgroundColor { get; set; }
            public bool fill { get; set; }
            public double tension { get; set; }
        }

        public class AddAmountRequest
        {
            public int? CategoryId { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost]
        public IActionResult AddAmount([FromBody] AddAmountRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return Json(new { success = false, message = "Amount must be greater than 0" });
                }

                var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimsUserId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                int userId = Convert.ToInt32(claimsUserId);
                
                _walletService.AddAmount(userId, request.Amount, request.CategoryId);
                var newBalance = _walletService.GetBalance(userId, request.CategoryId);
                var category = _categoryService.GetById(request.CategoryId.Value);

                return Json(new { 
                    success = true, 
                    newBalance = newBalance,
                    categoryId = request.CategoryId,
                    message = $"Successfully added ₱{request.Amount:N2} to {category.CategoryTitle}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding amount to wallet: {Message}", ex.Message);
                return Json(new { success = false, message = "An error occurred while adding funds." });
            }
        }

        [HttpPost]
        public IActionResult ResetBudget([FromBody] int? categoryId)
        {
            try
            {
                var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimsUserId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                int userId = Convert.ToInt32(claimsUserId);
                _walletService.ResetBalance(userId, categoryId);

                string budgetType = categoryId.HasValue ? 
                    _categoryService.GetById(categoryId.Value)?.CategoryTitle : 
                    "overall budget";

                return Json(new { 
                    success = true, 
                    message = $"Successfully reset {budgetType}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting budget");
                return Json(new { success = false, message = "An error occurred while resetting the budget" });
            }
        }

        [HttpGet]
        public IActionResult GetCategoryBalance(int categoryId)
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var balance = _walletService.GetBalance(userId, categoryId == 0 ? null : categoryId);
                
                return Json(new { success = true, balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category balance");
                return Json(new { success = false, message = "Error retrieving balance" });
            }
        }

        [HttpGet]
        public IActionResult GetTotalBudget()
        {
            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var categories = _categoryService.RetrieveAll(userId: userId);
                decimal totalBudget = 0;

                foreach (var category in categories)
                {
                    totalBudget += _walletService.GetBalance(userId, category.CategoryId);
                }

                return Json(new { success = true, totalBudget = totalBudget });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total budget");
                return Json(new { success = false, message = "Error retrieving total budget" });
            }
        }
    }

}
