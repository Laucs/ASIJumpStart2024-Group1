using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ICategoryRepository
    {
        IQueryable<MCategory> GetCategories();
        void AddCategory(MCategory model);
        void UpdateCategory(MCategory model);
        void DeleteCategory(int categoryId);
    }
}
