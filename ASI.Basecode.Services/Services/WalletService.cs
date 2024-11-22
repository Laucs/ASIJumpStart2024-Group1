using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger _logger;

    public WalletService(IWalletRepository walletRepository, ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _logger = logger;
    }

    public decimal GetBalance(int userId, int? categoryId = null)
    {
        try
        {
            return _walletRepository.GetCurrentBalance(userId, categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for user {UserId} and category {CategoryId}", userId, categoryId);
            throw;
        }
    }

    public void AddAmount(int userId, decimal amount, int? categoryId = null)
    {
        try
        {
            _walletRepository.AddAmount(userId, amount, categoryId);
            _logger.LogInformation("Successfully added {Amount} to wallet for user {UserId} and category {CategoryId}", 
                amount, userId, categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding amount {Amount} for user {UserId} and category {CategoryId}", 
                amount, userId, categoryId);
            throw new Exception("Failed to add funds to wallet", ex);
        }
    }

    public bool DeductExpense(int userId, decimal expenseAmount, int? categoryId = null)
    {
        var currentBalance = _walletRepository.GetCurrentBalance(userId, categoryId);
        if (currentBalance >= expenseAmount)
        {
            _walletRepository.UpdateBalance(userId, currentBalance - expenseAmount, categoryId);
            return true;
        }
        return false;
    }

    public void UpdateBalanceAfterExpenseRemoval(int userId, decimal expenseAmount, int? categoryId = null)
    {
        var currentBalance = _walletRepository.GetCurrentBalance(userId, categoryId);
        _walletRepository.UpdateBalance(userId, currentBalance + expenseAmount, categoryId);
    }

    public void ResetBalance(int userId, int? categoryId = null)
    {
        try
        {
            _walletRepository.ResetBalance(userId, categoryId);
            _logger.LogInformation("Reset balance for user {UserId} and category {CategoryId}", userId, categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting balance for user {UserId} and category {CategoryId}", 
                userId, categoryId);
            throw;
        }
    }

    public void DeleteWalletForCategory(int userId, int categoryId)
    {
        try
        {
            var wallet = _walletRepository.GetByCategoryAndUser(userId, categoryId);
            if (wallet != null)
            {
                _walletRepository.Delete(wallet);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wallet for category {CategoryId}", categoryId);
            throw;
        }
    }
} 
