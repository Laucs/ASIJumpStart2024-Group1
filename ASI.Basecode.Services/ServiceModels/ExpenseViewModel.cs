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
    namespace ASI.Basecode.Services.ServiceModels
    {
        public class ExpenseViewModel
        {
            [Required(ErrorMessage = "Amount is required.")]
            public int Amount { get; set; }

            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Description is required.")]
            public string Description { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime CreatedDate { get; set; }
        } 

    }
}