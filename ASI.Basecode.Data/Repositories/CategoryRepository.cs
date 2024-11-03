using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IEnumerable<MCategory> RetrieveAllCategories()
        {
            return GetDbSet<MCategory>().ToList();
        }


        // Get categories by user ID
        public IQueryable<MCategory> GetCategories(int? userId)
        {
            return this.GetDbSet<MCategory>().Where(x => x.UserId == userId).Include(x => x.MExpenses);   
        }

        public void AddCategory(MCategory model)
        {
            this.GetDbSet<MCategory>().Add(model);
            UnitOfWork.SaveChanges();
        }

        public MCategory RetrieveCategory(int categoryId) // Corrected spelling
        {
            return GetDbSet<MCategory>().FirstOrDefault(x => x.CategoryId == categoryId);
        }

        public void UpdateCategory(MCategory category)
        {
            var existingCategory = GetDbSet<MCategory>().Find(category.CategoryId);
   

            if (existingCategory != null)
            {
                existingCategory.CategoryTitle = category.CategoryTitle;
                UnitOfWork.SaveChanges();
            }
            else
            {
                throw new KeyNotFoundException("Category not found for update.");
            }
        }


        public void DeleteCategory(int categoryId) // Changed parameter to categoryId
        {
            var existingCategory = GetDbSet<MCategory>().Find(categoryId);
            if (existingCategory != null)
            {
                GetDbSet<MCategory>().Remove(existingCategory);
                UnitOfWork.SaveChanges();
            }
            else
            {
                throw new KeyNotFoundException("Category not found for deletion.");
            }
        }


    }
}
