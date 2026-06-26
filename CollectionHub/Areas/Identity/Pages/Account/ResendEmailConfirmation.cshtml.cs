#nullable disable

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
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                StatusMessage = "Se existir uma conta com esse email, será enviado um novo link de confirmação.";
                return RedirectToPage();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page("/Account/ConfirmEmail", null,
                new { area = "Identity", userId, code }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirmar email - CollectionHub",
                $"Confirme a sua conta clicando <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>aqui</a>.");

            StatusMessage = "Se existir uma conta com esse email, será enviado um novo link de confirmação.";
            return RedirectToPage();
        }
    }
}
