using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class CategoryServices : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryServices(IMapper mapper, ICategoryRepository expenseRepository)
        {
            _mapper = mapper;
            _categoryRepository = expenseRepository;
        }

        public IEnumerable<CategoryViewModel> RetrieveAll(int? id = null, int? userId = null)
        {
            var data = _categoryRepository.GetCategories().Where(x => x.UserId == userId);
            var model = data.Select(category => new CategoryViewModel
            {
                CategoryId = category.CategoryId,
                CategoryTitle = category.CategoryTitle,
                UserId = category.UserId,
            }).ToList();

            return model;
        }

        public CategoryViewModel RetreiveCategory(int id)
        {
            var data = _categoryRepository.GetCategories().FirstOrDefault(x => x.CategoryId == id);
            var model = new CategoryViewModel
            {
                CategoryId = data.CategoryId,
                CategoryTitle = data.CategoryTitle,
                UserId = data.UserId
            };
            return model;
        }

        public void Add(CategoryViewModel model)
        {
            var newModel = new MCategory();
            newModel.CategoryId = model.CategoryId;
            newModel.UserId = model.UserId;
            newModel.CategoryTitle = model.CategoryTitle;

            _categoryRepository.AddCategory(newModel);
        }

        public void Update(CategoryViewModel model)
        {
            var existingData = _categoryRepository.GetCategories().Where(c => c.CategoryId == model.CategoryId).FirstOrDefault();
            existingData.CategoryId = model.CategoryId;
            existingData.UserId = model.UserId;
            existingData.CategoryTitle = model.CategoryTitle;

            _categoryRepository.AddCategory(existingData);
        }

        public void Delete(int id)
        {
            _categoryRepository.DeleteCategory(id);
        }
    }
}
