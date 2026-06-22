using CollectionHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Services
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(int userId);
        Task<bool> DeductBalanceAsync(int userId, decimal amount);
        Task<bool> AddBalanceAsync(int userId, decimal amount);
    }

    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.WalletBalance ?? 0;
        }

        public async Task<bool> DeductBalanceAsync(int userId, decimal amount)
        {
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.WalletBalance < amount)
                return false;

            user.WalletBalance -= amount;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBalanceAsync(int userId, decimal amount)
        {
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return false;

            user.WalletBalance += amount;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}