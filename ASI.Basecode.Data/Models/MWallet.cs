using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class MWallet
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public decimal? Balance { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? CategoryId { get; set; }

        public virtual MUser User { get; set; }
    }
}
