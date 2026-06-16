using CollectionHub.Models;

namespace CollectionHub.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(CartItem item);
        void RemoveFromCart(int itemId);
        void ClearCart();
    }
}
