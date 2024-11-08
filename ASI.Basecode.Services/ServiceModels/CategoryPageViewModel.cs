using ASI.Basecode.Data.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    
        public class CategoryPageViewModel
    {
        public IEnumerable<CategoryViewModel> Categories { get; set; }
        public CategoryViewModel NewCategory { get; set; } = new CategoryViewModel();
    } 

    
}