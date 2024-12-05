using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class MTransaction
    {
        public int TransId { get; set; }
        public string CategoryName { get; set; }
        public DateTime? TransDate { get; set; }
        public int UserId { get; set; }
        public string AmountValue { get; set; }
        public string TransDescription { get; set; }

        public virtual MUser User { get; set; }
    }
}
