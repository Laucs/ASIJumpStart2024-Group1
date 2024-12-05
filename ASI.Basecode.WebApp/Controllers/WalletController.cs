using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Security.Claims;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;

namespace ASI.Basecode.WebApp.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly ITranscationService _transactionService;

        public WalletController(IWalletService walletService, ITranscationService transactionService)
        {
            _walletService = walletService;
            _transactionService = transactionService;
        }

        [HttpPost]
        public IActionResult AddAmount([FromBody] decimal amount)
        {
            try
            {
                var claimsUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId = Convert.ToInt32(claimsUserId);

                _walletService.AddAmount(userId, amount);
                var newBalance = _walletService.GetBalance(userId);
                return Json(new { success = true, newBalance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 