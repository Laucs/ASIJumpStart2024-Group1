using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IWalletService
    {
        decimal GetBalance(int userId, int? categoryId = null);
        void AddAmount(int userId, decimal amount, int? categoryId = null);
        bool DeductExpense(int userId, decimal expenseAmount, int? categoryId = null);
        void UpdateBalanceAfterExpenseRemoval(int userId, decimal expenseAmount, int? categoryId = null);
        void ResetBalance(int userId, int? categoryId = null);
        void DeleteWalletForCategory(int userId, int categoryId);
    }
}