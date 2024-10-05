using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    namespace ASI.Basecode.Services.ServiceModels
    {
        public class ExpenseViewModel
        {
            [Required(ErrorMessage = "Amount is required.")]
            public int Amount { get; set; }

            [Required(ErrorMessage = "Category is required.")]
            public int CategoryId { get; set; }


        }

    }
}