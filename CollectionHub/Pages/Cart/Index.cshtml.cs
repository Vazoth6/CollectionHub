using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CollectionHub.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;

        public IndexModel(ICartService cartService)
        {
            _cartService = cartService;
        }

        public ShoppingCart Cart { get; set; } = new();

        public void OnGet()
        {
            Cart = _cartService.GetCart();
        }

        public IActionResult OnPostUpdateQuantity(int itemId, int quantity)
        {
            _cartService.UpdateQuantity(itemId, quantity);
            TempData["Success"] = quantity <= 0
                ? "Item removido do carrinho."
                : "Quantidade atualizada com sucesso.";

            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int itemId)
        {
            _cartService.RemoveFromCart(itemId);
            TempData["Success"] = "Item removido do carrinho.";
            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            _cartService.ClearCart();
            TempData["Success"] = "Carrinho limpo com sucesso.";
            return RedirectToPage();
        }
    }
}
