using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<MTransaction> GetTransactions()
        {
            return this.GetDbSet<MTransaction>();
        }


        public void Add(MTransaction transaction)
        {
            var transactionSet = this.GetDbSet<MTransaction>();
            transactionSet.Add(transaction);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<MTransaction> GetByUserId(int userId)
        {
            var transactionSet = this.GetDbSet<MTransaction>();

            // Add debug logging
            var filteredTransactions = transactionSet.Where(t => t.UserId == userId);

            // Verify the exact conditions
            var transactionList = filteredTransactions.ToList();

            return filteredTransactions;
        }

    }

}
