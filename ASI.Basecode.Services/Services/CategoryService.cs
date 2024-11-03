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
using ASI.Basecode.Data;
using System.Diagnostics;

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


        public void Add(CategoryPageViewModel model)
        {
            if (model == null || model.NewCategory == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }

            var newCategoryTitle = model.NewCategory.CategoryTitle?.ToLower();
            var existingCategories = _categoryRepository.RetrieveAllCategories();

            // Check if category title already exists (case-insensitive)
            if (existingCategories.Any(c => c.CategoryTitle.ToLower() == newCategoryTitle))
            {
                throw new InvalidOperationException("A category with this title already exists.");
            }

            var newCategory = new MCategory
            {
                UserId = model.NewCategory.UserId,
                CategoryTitle = model.NewCategory.CategoryTitle
            };

            _categoryRepository.AddCategory(newCategory);
        }


        public CategoryViewModel RetrieveCategory(int id)
        {
            var data = _categoryRepository.RetrieveCategory(id); // Corrected spelling
            if (data == null) throw new KeyNotFoundException("Category not found.");

            return new CategoryViewModel
            {
                CategoryId = data.CategoryId,
                CategoryTitle = data.CategoryTitle,
                UserId = data.UserId
            };
        }

        public void Update(CategoryPageViewModel model)
        {
            if (model == null || model.NewCategory == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }

            var categoryId = model.NewCategory.CategoryId;
            var categoryTitle = model.NewCategory.CategoryTitle?.ToLower();

            Debug.WriteLine("Service Layer - Category ID: " + categoryId);
            Debug.WriteLine("Service Layer - Category Title: " + categoryTitle);

            var existingCategory = _categoryRepository.RetrieveCategory(categoryId);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            }

            // Check if another category with the same title already exists
            var otherCategories = _categoryRepository.RetrieveAllCategories()
                                .Where(c => c.CategoryId != categoryId);

            if (otherCategories.Any(c => c.CategoryTitle.ToLower() == categoryTitle))
            {
                throw new InvalidOperationException("A category with this title already exists.");
            }

            existingCategory.CategoryTitle = model.NewCategory.CategoryTitle;
            _categoryRepository.UpdateCategory(existingCategory);
        }

        public void Delete(int categoryId)
        {
            var category = _categoryRepository.RetrieveCategory(categoryId);
            if (category != null)
            {
                _categoryRepository.DeleteCategory(categoryId); // Pass categoryId directly
            }
            else
            {
                throw new KeyNotFoundException("Category not found for deletion.");
            }
        }

       


    }
}
