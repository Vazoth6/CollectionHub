using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<UserAdminDto> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.MyUsers
                .Select(u => new UserAdminDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = _context.Users.FirstOrDefault(i => i.Id == u.UserID).Email ?? "N/A",
                    CellPhone = u.CellPhone,
                    Role = u.Role,
                    RegisterDate = u.RegisterDate
                })
                .OrderByDescending(u => u.RegisterDate)
                .ToListAsync();
        }

        // ⭐ MÉTODO POST PARA ELIMINAR UTILIZADOR DIRETAMENTE
        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            try
            {
                var myUser = await _context.MyUsers
                    .Include(u => u.UserItems)
                    .Include(u => u.Sales)
                    .Include(u => u.Purchases)
                    .FirstOrDefaultAsync(m => m.Id == userId);

                if (myUser == null)
                {
                    TempData["Error"] = "Utilizador não encontrado.";
                    return RedirectToPage();
                }

                var identityUser = await _userManager.FindByIdAsync(myUser.UserID);

                // Verificar transações ativas
                var hasActiveTransactions = myUser.Sales.Any(s => s.Status != "Entregue" && s.Status != "Cancelada") ||
                                            myUser.Purchases.Any(p => p.Status != "Entregue" && p.Status != "Cancelada");

                if (hasActiveTransactions)
                {
                    TempData["Error"] = "Não é possível eliminar um utilizador com transações pendentes.";
                    return RedirectToPage();
                }

                // Remover dependências
                if (myUser.UserItems.Any())
                {
                    _context.UserItems.RemoveRange(myUser.UserItems);
                }

                if (myUser.Sales.Any())
                {
                    _context.Transactions.RemoveRange(myUser.Sales);
                }

                if (myUser.Purchases.Any())
                {
                    _context.Transactions.RemoveRange(myUser.Purchases);
                }

                var userLikes = _context.ItemLikes.Where(l => l.UserId == userId);
                if (userLikes.Any())
                {
                    _context.ItemLikes.RemoveRange(userLikes);
                }

                _context.MyUsers.Remove(myUser);
                await _context.SaveChangesAsync();

                // Eliminar IdentityUser
                if (identityUser != null)
                {
                    var result = await _userManager.DeleteAsync(identityUser);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        TempData["Warning"] = $"MyUser eliminado, mas erro ao eliminar conta de autenticação: {errors}";
                        return RedirectToPage();
                    }
                }

                TempData["Success"] = $"Utilizador {myUser.Name} eliminado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao eliminar utilizador: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

    public class UserAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CellPhone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
    }
}