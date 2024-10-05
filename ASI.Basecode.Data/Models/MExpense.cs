using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class MExpense
    {
        public int ExpenseId { get; set; }
        public int Amount { get; set; }
        public DateTime? DateCreated { get; set; }
        public string ExpenseDescription { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }

        public virtual MCategory Category { get; set; }
        public virtual MUser User { get; set; }
    }
}
