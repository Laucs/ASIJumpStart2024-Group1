using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IExpenseRepository
    {
        IQueryable<MExpense> GetExpenses();
        public bool ExpenseExists(int expenseId);
        void AddExpense(MExpense expense);
        void UpdateExpense(MExpense expense);
        void DeleteExpense(int expenseId);
    }
}
