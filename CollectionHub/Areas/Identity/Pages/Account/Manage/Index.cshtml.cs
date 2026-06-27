using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
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
    // Representa o modelo de dados utilizado pelo index model.
    // </summary>
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        // <summary>
        // Obtém ou define input.
        // </summary>
        public InputModel Input { get; set; }

        // <summary>
        // Representa o modelo de dados utilizado pelo input model.
        // </summary>
        public class InputModel
        {
            // NOME (do MyUser)
            [Required(ErrorMessage = "O nome é obrigatório")]
            [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
            [Display(Name = "Nome")]
            // <summary>
            // Obtém ou define nome.
            // </summary>
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Email")]
            // <summary>
            // Obtém ou define email.
            // </summary>
            public string Email { get; set; } = string.Empty;

            // TELEMÓVEL (do MyUser)
            [Display(Name = "Telemóvel")]
            [StringLength(20)]
            [RegularExpression(@"^(\+[0-9]{1,3})?[0-9]{9,12}$",
                ErrorMessage = "O número de telemóvel deve conter apenas dígitos e pode começar opcionalmente com um + e o indicativo do país.")]
            [Phone(ErrorMessage = "Formato de telemóvel inválido")]
            // <summary>
            // Obtém ou define número de telemóvel.
            // </summary>
            public string? CellPhone { get; set; }

            // CARGO (do MyUser)
            [Display(Name = "Cargo")]
            // <summary>
            // Obtém ou define cargo.
            // </summary>
            public string Role { get; set; } = string.Empty;

            // DATA DE REGISTO (do MyUser)
            [Display(Name = "Membro desde")]
            // <summary>
            // Obtém ou define data de registo.
            // </summary>
            public DateTime RegisterDate { get; set; }

            // SALDO (do MyUser)
            [Display(Name = "Saldo")]
            // <summary>
            // Obtém ou define saldo.
            // </summary>
            public decimal WalletBalance { get; set; }

            // ALTERA PASSWORD
            [Display(Name = "Alterar Password")]
            // <summary>
            // Obtém ou define a palavra-passe.
            // </summary>
            public bool ChangePassword { get; set; }

            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "A password deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
            [Display(Name = "Nova Password")]
            // <summary>
            // Obtém ou define nova palavra-passe.
            // </summary>
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "As passwords não coincidem.")]
            [Display(Name = "Confirmar Nova Password")]
            // <summary>
            // Obtém ou define confirmação da nova palavra-passe.
            // </summary>
            public string? ConfirmNewPassword { get; set; }
        }

        protected async Task LoadAsync(IdentityUser user)
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                throw new InvalidOperationException($"Não foi possível carregar o utilizador com ID '{_userManager.GetUserId(User)}'.");
            }

            // Procura o MyUser associado
            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.UserID == identityUser.Id);

            if (myUser == null)
            {
                // Cria MyUser se não existir
                myUser = new MyUser
                {
                    Name = identityUser.Email?.Split('@')[0] ?? "Utilizador",
                    Role = "Utilizador",
                    UserID = identityUser.Id,
                    WalletBalance = 100.00m,
                    RegisterDate = DateTime.Now
                };
                _context.MyUsers.Add(myUser);
                await _context.SaveChangesAsync();
            }

            Input = new InputModel
            {
                Name = myUser.Name,
                Email = identityUser.Email ?? string.Empty,
                CellPhone = myUser.CellPhone,
                Role = myUser.Role,
                RegisterDate = myUser.RegisterDate,
                WalletBalance = myUser.WalletBalance,
                ChangePassword = false,
                NewPassword = null,
                ConfirmNewPassword = null
            };
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

            await LoadAsync(user);
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

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Buscar o MyUser
            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.UserID == user.Id);

            if (myUser == null)
            {
                myUser = new MyUser
                {
                    Name = Input.Name,
                    Role = "Utilizador",
                    UserID = user.Id,
                    WalletBalance = 100.00m,
                    RegisterDate = DateTime.Now
                };
                _context.MyUsers.Add(myUser);
            }
            else
            {
                // ATUALIZA CAMPOS DO MyUser
                myUser.Name = Input.Name;
                myUser.CellPhone = Input.CellPhone;
            }

            await _context.SaveChangesAsync();

            // ALTERA PASSWORD (se solicitado)
            if (Input.ChangePassword && !string.IsNullOrEmpty(Input.NewPassword))
            {
                // Primeiro, verifica se a password atual está correta
                var hasPassword = await _userManager.HasPasswordAsync(user);
                if (hasPassword)
                {
                    // Para alterar a password, precisamos da password atual.
                    // 1. Remove a password atual
                    // 2. Adiciona a nova password
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
                        if (!addPasswordResult.Succeeded)
                        {
                            foreach (var error in addPasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            await LoadAsync(user);
                            return Page();
                        }
                    }
                    else
                    {
                        foreach (var error in removePasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await LoadAsync(user);
                        return Page();
                    }
                }
                else
                {
                    // Se o utilizador não tiver password adicionar diretamente
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
                    if (!addPasswordResult.Succeeded)
                    {
                        foreach (var error in addPasswordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await LoadAsync(user);
                        return Page();
                    }
                }

                _logger.LogInformation("Password alterada com sucesso.");
                await _signInManager.RefreshSignInAsync(user);
            }

            _logger.LogInformation("Perfil actualizado com sucesso.");
            TempData["Success"] = "Perfil actualizado com sucesso!";
            await LoadAsync(user);
            return RedirectToPage();
        }
    }
}
