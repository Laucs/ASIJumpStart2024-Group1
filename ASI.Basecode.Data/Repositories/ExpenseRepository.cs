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
        public ExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public IQueryable<MExpense> GetExpenses()
        {
            return this.GetDbSet<MExpense>();
        }



        public void AddExpense(MExpense model)
        {

            model.DateCreated = DateTime.Now; 
            this.GetDbSet<MExpense>().Add(model);
            // Save changes to persist the new expense in the database
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
                UnitOfWork.SaveChanges();
            }
            
        }




    }
}
