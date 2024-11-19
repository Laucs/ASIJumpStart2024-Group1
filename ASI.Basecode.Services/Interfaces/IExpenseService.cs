    using ASI.Basecode.Services.ServiceModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace ASI.Basecode.Services.Interfaces
    {
        public interface IExpenseService
        {
            public IEnumerable<ExpenseViewModel> RetrieveAll(int? id = null, int? userId = null);
            ExpenseViewModel RetrieveExpense(int id);
            void Add(ExpenseViewModel model);
            void Update(ExpenseViewModel model);
            void Delete(int id);
            void DeleteExpensesByCategoryId(int categoryId);
            ExpenseViewModel GetExpenseById(int expenseId);
        }
    }
