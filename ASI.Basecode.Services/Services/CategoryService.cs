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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(IMapper mapper, ICategoryRepository expenseRepository)
        {
            _mapper = mapper;
            _categoryRepository = expenseRepository;
        }

        public IEnumerable<CategoryViewModel> RetrieveAll(int? userId)
        {
            var data = _categoryRepository.GetCategories(userId);
            var model = data.Select(category => new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryTitle = category.CategoryTitle,
                UserId = category.UserId,
                MExpenses = category.MExpenses.Select(expense => new MExpense
                {
                    ExpenseId = expense.ExpenseId,
                    ExpenseName = expense.ExpenseName,
                    Amount = expense.Amount,
                    ExpenseDescription = expense.ExpenseDescription,
                    DateCreated = expense.DateCreated,
                    CategoryId = expense.CategoryId,
                    UserId = expense.UserId
                }).ToList()
            }).ToList();

            return model;
        }



        public CategoryViewModel RetreiveCategory(int id)
        {
            var data = _categoryRepository.GetCategories(id).FirstOrDefault(x => x.CategoryId == id);
            var model = new CategoryViewModel
            {
                CategoryId = data.CategoryId,
                CategoryTitle = data.CategoryTitle,
                UserId = data.UserId
            };
            return model;
        }

        public void Add(CategoryPageViewModel model)
        {
            var newCategory = new MCategory
            {
                UserId = model.NewCategory.UserId,
                CategoryTitle = model.NewCategory.CategoryTitle
            };

            _categoryRepository.AddCategory(newCategory);
        }

        /*public void Update(CategoryViewModel model)
        {
            var existingData = _categoryRepository.GetCategories().Where(c => c.CategoryId == model.CategoryId).FirstOrDefault();
            existingData.CategoryId = model.CategoryId;
            existingData.UserId = model.UserId;
            existingData.CategoryTitle = model.CategoryTitle;

            _categoryRepository.AddCategory(existingData);
        }*/

        public void Delete(int id)
        {
            _categoryRepository.DeleteCategory(id);
        }
    }
}
