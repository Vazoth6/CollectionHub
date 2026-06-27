#nullable disable

using System.Text;
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
    // Representa o modelo de dados utilizado pelo register confirmation model.
    // </summary>
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public RegisterConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender sender, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        // <summary>
        // Obtém ou define email.
        // </summary>
        public string Email { get; set; }
        // <summary>
        // Obtém ou define confirm account link.
        // </summary>
        public bool DisplayConfirmAccountLink { get; set; }
        // <summary>
        // Obtém ou define email confirmation url.
        // </summary>
        public string EmailConfirmationUrl { get; set; }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null) return RedirectToPage("/Index");
            returnUrl ??= Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound($"Não foi possível carregar o utilizador com email '{email}'.");

            Email = email;
            DisplayConfirmAccountLink = _environment.IsDevelopment();

            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page("/Account/ConfirmEmail", null,
                    new { area = "Identity", userId, code, returnUrl }, Request.Scheme);
            }

            return Page();
        }
    }
}
