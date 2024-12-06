using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Manager;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASI.Basecode.Data;
using System.Diagnostics;
using ASI.Basecode.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.Services.Services
{
    public class TransactionService : ITranscationService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public IEnumerable<TransactionViewModel> RetrieveAll(int? id = null, int? userId = null)
        {
            try
            {
                var data = _transactionRepository.GetTransactions()
                    .Where(x => userId == null || x.UserId == userId);

                var model = data
                    .OrderByDescending(x => x.TransDate)
                    .Select(transaction => new TransactionViewModel
                    {
                        UserId = transaction.UserId,
                        CategoryName = transaction.CategoryName,
                        TransDate = transaction.TransDate,
                        TransDescription = transaction.TransDescription,
                        AmountValue = transaction.AmountValue
                    })
                    .ToList();

                return model;
            }
            catch (Exception ex)
            {
                return new List<TransactionViewModel>();
            }
        }

        public void AddTransaction(int userId, int categoryId, decimal amount, bool isAddition, string description)
        {
            try
            {
                // Retrieve the category details
                var category = _categoryRepository.GetById(categoryId);
                if (category == null)
                {
                    throw new ArgumentException($"Category with ID {categoryId} not found");
                }

                // Adjust the transaction amount and format
                string formattedAmount = isAddition ? $"+ {amount:N2}" : $"- {amount:N2}";

                // Create the transaction entity
                var transaction = new MTransaction
                {
                    UserId = userId,
                    CategoryName = category.CategoryTitle,
                    AmountValue = formattedAmount,
                    TransDescription = description,
                    TransDate = DateTime.UtcNow
                 
                };

                // Add the transaction to the database
                _transactionRepository.Add(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding transaction for user {userId} and category {categoryId}");
                throw;
            }
        }

    }


}
