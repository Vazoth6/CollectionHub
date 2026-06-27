using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    // <summary>
    // Representa o modelo de dados utilizado pelo personal data model.
    // </summary>
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonalDataModel> _logger;

        public PersonalDataModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        // <summary>
        // Processa o formulário submetido pelo utilizador.
        // </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Não foi possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
            }

            var userId = await _userManager.GetUserIdAsync(user);

            try
            {
                // 1. OBTÉM O MyUser ASSOCIADO
                var myUser = await _context.MyUsers
                    .Include(u => u.UserItems)
                    .Include(u => u.Sales)
                    .Include(u => u.Purchases)
                    .FirstOrDefaultAsync(m => m.UserID == userId);

                if (myUser != null)
                {
                    // 2. REMOVE ITEMS DO UTILIZADOR (UserItems)
                    if (myUser.UserItems.Any())
                    {
                        _context.UserItems.RemoveRange(myUser.UserItems);
                    }

                    // 3. REMOVE TRANSAÇÕES (Vendas e Compras)
                    if (myUser.Sales.Any())
                    {
                        // Devolve items ao estado disponível
                        foreach (var sale in myUser.Sales)
                        {
                            if (sale.Item != null && sale.Status != "Concluída")
                            {
                                sale.Item.Status = "Disponível";
                            }
                        }
                        _context.Transactions.RemoveRange(myUser.Sales);
                    }

                    if (myUser.Purchases.Any())
                    {
                        _context.Transactions.RemoveRange(myUser.Purchases);
                    }

                    // 4. REMOVE O MyUser
                    _context.MyUsers.Remove(myUser);
                }

                // 5. REMOVE O IdentityUser
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Utilizador '{user.Email}' eliminou a sua conta.");

                    // Faz logout e redireciona
                    await _signInManager.SignOutAsync();
                    TempData["Success"] = "A sua conta foi eliminada com sucesso.";
                    return RedirectToPage("/Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    _logger.LogWarning($"Erro ao eliminar conta: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao eliminar conta: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao eliminar a sua conta. Tente novamente mais tarde.");
                return Page();
            }
        }
    }
}
