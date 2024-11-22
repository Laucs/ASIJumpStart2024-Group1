using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class WalletRepository : BaseRepository, IWalletRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public MWallet GetWalletByUserId(int userId)
        {
            return GetAll().FirstOrDefault(w => w.UserId == userId && w.CategoryId == null);
        }

        public MWallet GetWalletByUserAndCategory(int userId, int categoryId)
        {
            return GetAll().FirstOrDefault(w => w.UserId == userId && w.CategoryId == categoryId);
        }

        public decimal GetCurrentBalance(int userId, int? categoryId = null)
        {
            var wallet = categoryId.HasValue ? 
                GetWalletByUserAndCategory(userId, categoryId.Value) : 
                GetWalletByUserId(userId);
            return wallet?.Balance ?? 0;
        }

        public void UpdateBalance(int userId, decimal newBalance, int? categoryId = null)
        {
            try
            {
                var wallet = categoryId.HasValue ? 
                    GetWalletByUserAndCategory(userId, categoryId.Value) : 
                    GetWalletByUserId(userId);

                if (wallet == null)
                {
                    wallet = new MWallet
                    {
                        UserId = userId,
                        CategoryId = categoryId,
                        Balance = newBalance,
                        LastUpdated = DateTime.UtcNow
                    };
                    this.GetDbSet<MWallet>().Add(wallet);
                }
                else
                {
                    wallet.Balance = newBalance;
                    wallet.LastUpdated = DateTime.UtcNow;
                    this.GetDbSet<MWallet>().Update(wallet);
                }

                UnitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Non-critical error in UpdateBalance: {ex.Message}");
                throw;
            }
        }

        public void AddAmount(int userId, decimal amount, int? categoryId = null)
        {
            try
            {
                var wallet = categoryId.HasValue ? 
                    GetWalletByUserAndCategory(userId, categoryId.Value) : 
                    GetWalletByUserId(userId);

                if (wallet == null)
                {
                    wallet = new MWallet
                    {
                        UserId = userId,
                        CategoryId = categoryId,
                        Balance = amount,
                        LastUpdated = DateTime.UtcNow
                    };
                    this.GetDbSet<MWallet>().Add(wallet);
                }
                else
                {
                    wallet.Balance += amount;
                    wallet.LastUpdated = DateTime.UtcNow;
                    this.GetDbSet<MWallet>().Update(wallet);
                }

                UnitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add amount: {ex.Message}", ex);
            }
        }

        public void ResetBalance(int userId, int? categoryId = null)
        {
            try
            {
                var wallet = categoryId.HasValue ? 
                    GetWalletByUserAndCategory(userId, categoryId.Value) : 
                    GetWalletByUserId(userId);

                if (wallet != null)
                {
                    wallet.Balance = 0;
                    wallet.LastUpdated = DateTime.UtcNow;
                    this.GetDbSet<MWallet>().Update(wallet);
                    UnitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Non-critical error in ResetBalance: {ex.Message}");
                throw;
            }
        }

        public MWallet GetByCategoryAndUser(int userId, int categoryId)
        {
            return this.GetDbSet<MWallet>()
                .FirstOrDefault(w => w.UserId == userId && w.CategoryId == categoryId);
        }

        public void Delete(MWallet wallet)
        {
            if (wallet != null)
            {
                this.GetDbSet<MWallet>().Remove(wallet);
                this.UnitOfWork.SaveChanges();
            }
        }

        private IQueryable<MWallet> GetAll()
        {
            return this.GetDbSet<MWallet>();
        }
    }
}
