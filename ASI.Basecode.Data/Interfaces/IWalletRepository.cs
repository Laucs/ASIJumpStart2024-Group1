using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IWalletRepository
    {
        MWallet GetWalletByUserId(int userId);
        MWallet GetWalletByUserAndCategory(int userId, int categoryId);
        void UpdateBalance(int userId, decimal newBalance, int? categoryId = null);
        decimal GetCurrentBalance(int userId, int? categoryId = null);
        void AddAmount(int userId, decimal amount, int? categoryId = null);
        void ResetBalance(int userId, int? categoryId = null);
        MWallet GetByCategoryAndUser(int userId, int categoryId);
        void Delete(MWallet wallet);
    }
}