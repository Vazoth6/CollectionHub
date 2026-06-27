using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace CollectionHub.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    // <summary>
    // Representa o modelo de dados utilizado pelo error model.
    // </summary>
    public class ErrorModel : PageModel
    {
        // <summary>
        // Obtém ou define o identificador do pedido.
        // </summary>
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }

}
