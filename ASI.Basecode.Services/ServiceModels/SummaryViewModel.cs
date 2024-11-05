using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class SummaryViewModel
    {
        public List<CategoryViewModel> CategoryAnalytics { get; set; }
        public List<ExpenseViewModel> ExpenseAnalytics { get; set; }
        public int TotalExpenses { get; set; }
    }

}
