using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Security.Claims;

namespace ASI.Basecode.WebApp.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
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