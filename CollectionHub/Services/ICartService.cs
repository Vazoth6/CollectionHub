using CollectionHub.Models;

namespace CollectionHub.Services
{
    public interface ICartService
    {
        ShoppingCart GetCart();
        void AddToCart(CartItem item);
        void UpdateQuantity(int itemId, int quantity);
        void RemoveFromCart(int itemId);
        void ClearCart();
    }
}
