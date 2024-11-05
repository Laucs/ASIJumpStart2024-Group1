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
    
        public class CategoryViewModel
        {
            [Required(ErrorMessage = "Category title is required.")]
            public string CategoryTitle { get; set; }

            public int CategoryId { get; set; }

            [Required(ErrorMessage = "User is required.")]
            public int UserId { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime CreatedDate { get; set; }

            public virtual MUser User { get; set; }

            //public virtual ICollection<MExpense> MExpenses { get; set; }
            public virtual ICollection<MExpense> MExpenses { get; set; } = new List<MExpense>();
            public int TotalAmount { get; set; }

    }
}