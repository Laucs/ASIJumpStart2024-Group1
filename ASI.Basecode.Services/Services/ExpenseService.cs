using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMapper _mapper;

        public ExpenseService(IMapper mapper, IExpenseRepository expenseRepository)
        {
            _mapper = mapper;
            _expenseRepository = expenseRepository;
        }


        ///// <summary>
        ///// Retrieves all.
        ///// </summary>
        ///// <returns></returns>

        public IEnumerable<ExpenseViewModel> RetrieveAll(int? id = null, int? userId = null)
        {
            var data = _expenseRepository.GetExpenses()
                .Where(x => x.UserId == userId);

            var model = data.Select(expense => new ExpenseViewModel
            {
                CategoryId = expense.CategoryId,
                Amount = expense.Amount,
                ExpenseName = expense.ExpenseName,
                CreatedDate = expense.DateCreated ?? DateTime.MinValue,
                Description = expense.ExpenseDescription,
            }).ToList();

            return model;
        }

        public ExpenseViewModel RetrieveExpense(int id)
        {
            var data = _expenseRepository.GetExpenses().FirstOrDefault(x => x.ExpenseId == id);
            var model = new ExpenseViewModel
            {
                CategoryId = data.CategoryId,
                Amount = data.Amount,
                CreatedDate = data.DateCreated ?? DateTime.MinValue,
                Description = data.ExpenseDescription,
            };
            return model;
        }

        ///// <summary>
        ///// Adds the specified model.
        ///// </summary>
        ///// <param name="model">The model.</param>
        public void Add(ExpenseViewModel model)
        {
            // Create a new MExpense object to map the data from the view model
            var newModel = new MExpense
            {
                CategoryId = model.CategoryId,
                Amount = model.Amount,
                ExpenseDescription = model.Description,
                DateCreated = model.CreatedDate,
                UserId = model.UserId,  // Ensure this is correctly assigned
                ExpenseName = model.ExpenseName // Newly added field
            };

            // Call the repository method to save the expense to the database
            _expenseRepository.AddExpense(newModel);
        }


        ///// <summary>
        ///// Updates the specified model.
        ///// </summary>
        ///// <param name="model">The model.</param>
        public void Update(ExpenseViewModel model)
        {
            var existingData = _expenseRepository.GetExpenses().Where(s => s.ExpenseId == model.ExpenseId).FirstOrDefault();
            existingData.CategoryId = model.CategoryId;
            existingData.Amount = model.Amount;
            existingData.ExpenseDescription = model.Description;
            existingData.DateCreated = model.CreatedDate;

            _expenseRepository.UpdateExpense(existingData);

        }

        ///// <summary>
        ///// Deletes the specified identifier.
        ///// </summary>
        ///// <param name="id">The identifier.</param>
        public void Delete(int id)
        {
            _expenseRepository.DeleteExpense(id);
        }
    }
}
