using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;

namespace CollectionHub.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalItems { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<RecentActivity> RecentActivities { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.MyUsers.CountAsync();
            TotalItems = await _context.Items.CountAsync();
            TotalTransactions = await _context.Transactions.CountAsync();
            TotalRevenue = await _context.Transactions
                .Where(t => t.Status == "Concluída")
                .SumAsync(t => t.Price);

            // Carregar atividades recentes (últimas 10 transações)
            var recentTransactions = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToListAsync();

            RecentActivities = recentTransactions.Select(t => new RecentActivity
            {
                Date = t.Date,
                Description = $"{t.Buyer?.Name ?? "Utilizador"} comprou '{t.Item?.Name ?? "Item"}' de {t.Seller?.Name ?? "Vendedor"}",
                Icon = "bi-cart-check"
            }).ToList();

            // Adicionar novos utilizadores à atividade
            var recentUsers = await _context.MyUsers
                .OrderByDescending(u => u.RegisterDate)
                .Take(5)
                .ToListAsync();

            RecentActivities.AddRange(recentUsers.Select(u => new RecentActivity
            {
                Date = u.RegisterDate,
                Description = $"Novo utilizador registado: {u.Name}",
                Icon = "bi-person-plus"
            }));

            RecentActivities = RecentActivities
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();
        }
    }

    public class RecentActivity
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-info-circle";
    }
}