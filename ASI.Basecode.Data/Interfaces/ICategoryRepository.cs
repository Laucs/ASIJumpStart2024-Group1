using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ICategoryRepository
    {
        IQueryable<MExpense> GetCategories();

        void AddCategory(MExpense expense);
        void UpdateCategory(MExpense expense);
        void DeleteCategory(int expenseId);
    }
}
