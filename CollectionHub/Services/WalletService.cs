using CollectionHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Services
{
    // <summary>
    // Representa iwallet service no domínio da aplicação.
    // </summary>
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync(int userId);
        Task<bool> DeductBalanceAsync(int userId, decimal amount);
        Task<bool> AddBalanceAsync(int userId, decimal amount);
    }

    // <summary>
    // Serviço responsável por operações relacionadas com a carteira e saldo dos utilizadores.
    // </summary>
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;

        public WalletService(ApplicationDbContext context)
        {
            _context = context;
        }

        // <summary>
        // Obtém o saldo da carteira de um utilizador.
        // </summary>
        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.WalletBalance ?? 0;
        }

        // <summary>
        // Executa a operação de dedução de saldo.
        // </summary>
        public async Task<bool> DeductBalanceAsync(int userId, decimal amount)
        {
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.WalletBalance < amount)
                return false;

            user.WalletBalance -= amount;
            await _context.SaveChangesAsync();
            return true;
        }

        // <summary>
        // Adiciona saldo à carteira de um utilizador.
        // </summary>
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
