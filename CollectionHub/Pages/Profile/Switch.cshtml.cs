using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CollectionHub.Pages.Profile
{
    [Authorize]
    // <summary>
    // Representa o modelo de dados utilizado pelo switch model.
    // </summary>
    public class SwitchModel : PageModel
    {
        private static readonly string[] AllowedProfiles = { "Comprador", "Vendedor" };

        // <summary>
        // Processa o formulário submetido pelo utilizador.
        // </summary>
        public IActionResult OnPost(string profile, string? returnUrl = null)
        {
            if (!AllowedProfiles.Contains(profile))
            {
                profile = "Comprador";
            }

            HttpContext.Session.SetString("ActiveProfile", profile);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Inventory/Index");
        }
    }
}
