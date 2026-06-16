using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
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

        public CollectionHub.Models.Cart Cart { get; set; } = new();

        public void OnGet()
        {
            Cart = _cartService.GetCart();
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
