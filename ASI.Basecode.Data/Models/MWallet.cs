using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models
{
    public partial class MWallet
    {
        public int WalletId { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        
        public DateTime LastUpdated { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("UserId")]
        public virtual MUser User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual MCategory Category { get; set; }
    }
}
