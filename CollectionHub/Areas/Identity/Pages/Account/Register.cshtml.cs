using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    // <summary>
    // Representa o modelo de dados utilizado pelo register model.
    // </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        // <summary>
        // Obtém ou define input.
        // </summary>
        public InputModel Input { get; set; }

        // <summary>
        // Obtém ou define URL de retorno.
        // </summary>
        public string ReturnUrl { get; set; }

        // <summary>
        // Obtém ou define external logins.
        // </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // <summary>
        // Representa o modelo de dados utilizado pelo input model.
        // </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório")]
            [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
            [Display(Name = "Nome completo")]
            // <summary>
            // Obtém ou define nome.
            // </summary>
            public string Name { get; set; }

            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            [Display(Name = "Email")]
            // <summary>
            // Obtém ou define email.
            // </summary>
            public string Email { get; set; }

            [Phone(ErrorMessage = "Formato de telemóvel inválido")]
            [Display(Name = "Telemóvel")]
            // <summary>
            // Obtém ou define número de telemóvel.
            // </summary>
            public string? CellPhone { get; set; }

            [Required(ErrorMessage = "A senha é obrigatória")]
            [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            // <summary>
            // Obtém ou define palavra-passe.
            // </summary>
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
            // <summary>
            // Obtém ou define palavra-passe de confirmação.
            // </summary>
            public string ConfirmPassword { get; set; }
        }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // <summary>
        // Processa o formulário submetido pelo utilizador.
        // </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with palavra-passe.");

                    // CRIA O MyUser ASSOCIADO
                    var myUser = new MyUser
                    {
                        Name = Input.Name,
                        CellPhone = Input.CellPhone,
                        Role = "Utilizador",
                        RegisterDate = DateTime.Now,
                        UserID = user.Id,
                        WalletBalance = 5000.00m
                    };

                    _context.MyUsers.Add(myUser);
                    await _context.SaveChangesAsync();

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirme o seu email",
                        $"Por favor, confirme a sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'.");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
