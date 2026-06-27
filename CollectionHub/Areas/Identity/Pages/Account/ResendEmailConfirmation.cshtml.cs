using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CollectionHub.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    // <summary>
    // Representa o modelo de dados utilizado pelo resend email confirmation model.
    // </summary>
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        // <summary>
        // Obtém ou define input.
        // </summary>
        public InputModel Input { get; set; }

        // ADICIONA A PROPRIEDADE StatusMessage
        [TempData]
        // <summary>
        // Obtém ou define a mensagem de estado.
        // </summary>
        public string StatusMessage { get; set; }

        // <summary>
        // Representa o modelo de dados utilizado pelo input model.
        // </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            // <summary>
            // Obtém ou define email.
            // </summary>
            public string Email { get; set; }
        }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public void OnGet()
        {
        }

        // <summary>
        // Processa o formulário submetido pelo utilizador.
        // </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email não encontrado.");
                return Page();
            }

            // Verifica se o email já está confirmado
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "Este email já foi confirmado.";
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirme o seu email",
                $"Por favor, confirme a sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

            StatusMessage = "Link de confirmação enviado. Verifique o seu email.";
            return Page();
        }
    }
}
