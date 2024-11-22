using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class ExpenseRepository : BaseRepository, IExpenseRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<MExpense> GetExpenses()
        {
            return this.GetDbSet<MExpense>();
        }



        public void AddExpense(MExpense model)
        {

            model.DateCreated = model.DateCreated; 
            this.GetDbSet<MExpense>().Add(model);
            // Save changes to persist the new expense in the database
            UnitOfWork.SaveChanges();
        }


        public void UpdateExpense(MExpense expense)
        {
            var existingExpense = GetDbSet<MExpense>().Find(expense.ExpenseId);

            if (existingExpense != null)
            {
                // Update the fields
                existingExpense.ExpenseName = expense.ExpenseName;
                existingExpense.Amount = expense.Amount;
                existingExpense.CategoryId = expense.CategoryId;
                existingExpense.DateCreated = expense.DateCreated;
                existingExpense.ExpenseDescription = expense.ExpenseDescription;

                // Mark entity as modified if necessary and save changes
                UnitOfWork.SaveChanges();
            }
            else
            {
                throw new KeyNotFoundException("Expense not found for update.");
            }
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

        public IEnumerable<MExpense> GetExpensesByCategoryId(int categoryId)
        {
            return GetDbSet<MExpense>()
                .Where(e => e.CategoryId == categoryId)
                .ToList();
        }

        public MExpense GetById(int expenseId)
        {
            return this.GetDbSet<MExpense>()
                .FirstOrDefault(e => e.ExpenseId == expenseId);
        }
    }
}
