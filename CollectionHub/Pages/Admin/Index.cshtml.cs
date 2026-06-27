using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;

namespace CollectionHub.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    // <summary>
    // Representa o modelo de dados utilizado pelo index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // <summary>
        // Obtém ou define total utilizadores.
        // </summary>
        public int TotalUsers { get; set; }
        // <summary>
        // Obtém ou define total artigos.
        // </summary>
        public int TotalItems { get; set; }
        // <summary>
        // Obtém ou define total transações.
        // </summary>
        public int TotalTransactions { get; set; }
        // <summary>
        // Obtém ou define total da receita.
        // </summary>
        public decimal TotalRevenue { get; set; }
        // <summary>
        // Obtém ou define atividades recentes.
        // </summary>
        public List<RecentActivity> RecentActivities { get; set; } = new();
        // <summary>
        // Obtém ou define confirmações de email pendentes.
        // </summary>
        public List<PendingEmailConfirmation> PendingEmailConfirmations { get; set; } = new();

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync()
        {
            TotalUsers = await _context.MyUsers.CountAsync();
            TotalItems = await _context.Items.CountAsync();
            TotalTransactions = await _context.Transactions.CountAsync();
            TotalRevenue = await _context.Transactions
                .Where(t => t.Status == "Concluída")
                .SumAsync(t => t.Price);

            // CARREGA CONFIRMAÇÕES DE EMAIL PENDENTES
            await LoadPendingEmailConfirmations();

            // Carrega atividades recentes (últimas 10 transações)
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

            // Adiciona novos utilizadores à atividade
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

        // CARREGA UTILIZADORES COM EMAIL NÃO CONFIRMADO
        private async Task LoadPendingEmailConfirmations()
        {
            var users = await _userManager.Users.ToListAsync();
            var pendingUsers = new List<PendingEmailConfirmation>();

            foreach (var user in users)
            {
                var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                if (!isEmailConfirmed)
                {
                    var myUser = await _context.MyUsers
                        .FirstOrDefaultAsync(m => m.UserID == user.Id);

                    pendingUsers.Add(new PendingEmailConfirmation
                    {
                        UserId = user.Id,
                        Name = myUser?.Name ?? user.Email?.Split('@')[0] ?? "Utilizador",
                        Email = user.Email ?? "N/A",
                        RegisterDate = myUser?.RegisterDate ?? DateTime.Now
                    });
                }
            }

            PendingEmailConfirmations = pendingUsers;
        }

        //  CONFIRMA EMAIL (POST)
        // <summary>
        // Executa a operação de confirmação de email.
        // </summary>
        public async Task<IActionResult> OnPostConfirmEmailAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ID do utilizador inválido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            // Gera token de confirmação
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                var myUser = await _context.MyUsers.FirstOrDefaultAsync(m => m.UserID == userId);
                TempData["Success"] = $"Email de '{myUser?.Name ?? user.Email}' confirmado com sucesso!";
            }
            else
            {
                TempData["Error"] = "Erro ao confirmar o email. Tente novamente.";
            }

            return RedirectToPage();
        }

        // REENVIA CONFIRMAÇÃO (POST)
        // <summary>
        // Executa a operação de reenvio de confirmação de email.
        // </summary>
        public async Task<IActionResult> OnPostResendConfirmationAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ID do utilizador inválido.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToPage();
            }

            // Gera novo token e enviar email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var myUser = await _context.MyUsers.FirstOrDefaultAsync(m => m.UserID == userId);
            TempData["Success"] = $"Email de confirmação reenviado para '{myUser?.Name ?? user.Email}'.";

            return RedirectToPage();
        }
    }

    // <summary>
    // Representa a atividade recente no domínio da aplicação.
    // </summary>
    public class RecentActivity
    {
        // <summary>
        // Obtém ou define data.
        // </summary>
        public DateTime Date { get; set; }
        // <summary>
        // Obtém ou define descrição.
        // </summary>
        public string Description { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define ícone.
        // </summary>
        public string Icon { get; set; } = "bi-info-circle";
    }

    // <summary>
    // Representa email não confirmado no domínio da aplicação.
    // </summary>
    public class PendingEmailConfirmation
    {
        // <summary>
        // Obtém ou define user id.
        // </summary>
        public string UserId { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define email.
        // </summary>
        public string Email { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define data de registo.
        // </summary>
        public DateTime RegisterDate { get; set; }
    }
}
