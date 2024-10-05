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
        public CategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
        public IQueryable<MCategory> GetCategories()
        {
            return this.GetDbSet<MCategory>();
        }

        public void AddCategory(MCategory model)
        {
            var maxId = this.GetDbSet<MCategory>().Max(x => x.CategoryId) + 1;
            model.CategoryId = maxId;
            UnitOfWork.SaveChanges();
        }

        public void UpdateCategory(MCategory model)
        {
            var existingData = this.GetDbSet<MCategory>().Where(x => x.CategoryId == model.CategoryId).FirstOrDefault();
            if (existingData != null)
            {
                this.GetDbSet<MCategory>().Update(model);
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteCategory(int categoryId)
        {
            var existingData = this.GetDbSet<MCategory>().Where(x => x.CategoryId == categoryId).FirstOrDefault();
            if (existingData != null)
            {
                this.GetDbSet<MCategory>().Remove(existingData);
            }
        }



       

    }
}
