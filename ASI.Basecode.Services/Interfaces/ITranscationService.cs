using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITranscationService
    {
        public IEnumerable<TransactionViewModel> RetrieveAll(int? id = null, int? userId = null);

        void AddTransaction(int userId, int categoryId, decimal amount, bool isAddition, string description);
    }

}
