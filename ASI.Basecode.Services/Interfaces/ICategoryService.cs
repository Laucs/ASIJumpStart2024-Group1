using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ICategoryService
    {
        public IEnumerable<CategoryViewModel> RetrieveAll(int? id = null, int? userId = null);
        CategoryViewModel RetreiveCategory(int id);
        void Add(CategoryViewModel model);
        void Update(CategoryViewModel model);
        void Delete(int id);
    }
}
