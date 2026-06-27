using CollectionHub.Models;

namespace CollectionHub.Services
{
    // <summary>
    // Contrato do serviço de carrinho, usado para desacoplar a implementação das páginas e controladores.
    // </summary>
    public interface ICartService
    {
        ShoppingCart GetCart();
        void AddToCart(CartItem item);
        void UpdateQuantity(int itemId, int quantity);
        void RemoveFromCart(int itemId);
        void ClearCart();
    }
}
