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
    public class CategoryRepository : BaseRepository, ICategoryRepository
    {
        private readonly List<MCategory> _data = new List<MCategory>();
        private int _nextId = 1;

        public CategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
        public IEnumerable<MCategory> GetCategories()
        {
            return _data;
        }

        public void AddCategory(MCategory model)
        {
            model.CategoryId = _nextId++;
            _data.Add(model);
        }

        public void UpdateCategory(MCategory model)
        {
            var existingData = _data.Where(x => x.CategoryId == model.CategoryId).FirstOrDefault();
            if (existingData != null)
            {
                existingData = model;
            }
        }

        public void DeleteCategory(int categoryId)
        {
            var existingData = _data.Where(x => x.CategoryId == categoryId).FirstOrDefault();
            if (existingData != null)
            {
                _data.Remove(existingData);
            }
        }



       

    }
}
