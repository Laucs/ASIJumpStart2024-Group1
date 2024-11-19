using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class WalletViewModel
{
    public decimal CurrentBalance { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal RemainingBalance { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal AmountToAdd { get; set; }
} 