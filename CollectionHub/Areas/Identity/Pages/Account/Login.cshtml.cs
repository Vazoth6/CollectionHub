#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CollectionHub.Areas.Identity.Pages.Account
{
    // <summary>
    // Representa o modelo de dados utilizado pelo login model.
    // </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        // <summary>
        // Obtém ou define input.
        // </summary>
        public InputModel Input { get; set; }

        // <summary>
        // Obtém ou define logins externos.
        // </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // <summary>
        // Obtém ou define URL de retorno.
        // </summary>
        public string ReturnUrl { get; set; }

        [TempData]
        // <summary>
        // Obtém ou define mensagem de erro.
        // </summary>
        public string ErrorMessage { get; set; }

        // <summary>
        // Representa o modelo de dados utilizado pelo input model.
        // </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            // <summary>
            // Obtém ou define email.
            // </summary>
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            // <summary>
            // Obtém ou define palavra-passe.
            // </summary>
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            // <summary>
            // Obtém ou define opção para manter a sessão iniciada.
            // </summary>
            public bool RememberMe { get; set; }
        }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
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
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return Page();
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Confirme o seu email antes de iniciar sessão. Verifique a sua caixa de entrada ou reenvie a confirmação.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa",
                        new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
            }

            return Page();
        }
    }
}
