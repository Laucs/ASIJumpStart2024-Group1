using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ICategoryService
    {
        public IEnumerable<CategoryViewModel> RetrieveAll(int? userId);
        CategoryViewModel RetreiveCategory(int id);
        void Add(CategoryPageViewModel model);
       /* void Update(CategoryViewModel model);*/
        void Delete(int id);
    }

  
}
