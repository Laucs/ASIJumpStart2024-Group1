using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class ExpenseRepository : BaseRepository, IExpenseRepository
    {
        private readonly List<MExpense> _data = new List<MExpense>();
        private int _nextId = 1;
        public ExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IEnumerable<MExpense> GetExpenses()
        {
            return this.GetDbSet<MExpense>();
        }

        public void AddExpense(MExpense model)
        {
            var maxId = this.GetDbSet<MUser>().Max(x => x.UserId) + 1;
            model.ExpenseId = maxId;
            model.DateCreated = DateTime.Now;
            this.GetDbSet<MExpense>().Add(model);
            UnitOfWork.SaveChanges();
        }

        public void UpdateExpense(MExpense model)
        {
            this.GetDbSet<MExpense>().Update(model);
            UnitOfWork.SaveChanges();

        }

        public void DeleteExpense(int expenseId)
        {
            var expenseToDelete = this.GetDbSet<MExpense>().FirstOrDefault(x => x.ExpenseId == expenseId);
            if (expenseToDelete != null)
            {
                this.GetDbSet<MExpense>().Remove(expenseToDelete);
            }
            
        }




    }
}
