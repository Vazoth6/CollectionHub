using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CollectionHub.Pages.Cart
{
    // <summary>
    // Representa o modelo de dados utilizado para o index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;

        public IndexModel(ICartService cartService)
        {
            _cartService = cartService;
        }

        // <summary>
        // Obtém ou define carrinho.
        // </summary>
        public ShoppingCart Cart { get; set; } = new();

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public void OnGet()
        {
            Cart = _cartService.GetCart();
        }

        // <summary>
        // Executa a operação de atualização da quantidade.
        // </summary>
        public IActionResult OnPostUpdateQuantity(int itemId, int quantity)
        {
            _cartService.UpdateQuantity(itemId, quantity);
            TempData["Success"] = quantity <= 0
                ? "Item removido do carrinho."
                : "Quantidade actualizada com sucesso.";

            return RedirectToPage();
        }

        // <summary>
        // Executa a operação de remoção.
        // </summary>
        public IActionResult OnPostRemove(int itemId)
        {
            _cartService.RemoveFromCart(itemId);
            TempData["Success"] = "Item removido do carrinho.";
            return RedirectToPage();
        }

        // <summary>
        // Executa a operação de limpeza.

        // </summary>
        public IActionResult OnPostClear()
        {
            _cartService.ClearCart();
            TempData["Success"] = "Carrinho limpo com sucesso.";
            return RedirectToPage();
        }
    }
}
