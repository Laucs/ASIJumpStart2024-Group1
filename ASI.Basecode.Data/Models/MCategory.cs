using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class MCategory
    {
        public MCategory()
        {
            MExpenses = new HashSet<MExpense>();
        }

        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        public int UserId { get; set; }

        public virtual MUser User { get; set; }
        public virtual ICollection<MExpense> MExpenses { get; set; }
    }
}
