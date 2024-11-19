using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class SummaryViewModel
    {
        public List<CategoryViewModel> SummaryAnalytics { get; set; }
        public CategoryPageViewModel CategoryAnalytics { get; set; }

        public List<ExpenseViewModel> ExpenseAnalytics { get; set; }
        public int TotalExpenses { get; set; }

        public decimal CurrentBalance { get; set; }
        public decimal TotalExpenseAmount { get; set; }
        public decimal RemainingBalance { get; set; }
    }

}
