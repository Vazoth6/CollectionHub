using System.Text.Json;
using CollectionHub.Models;

namespace CollectionHub.Services
{
    public class CartService : ICartService
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ShoppingCart GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = session?.GetString(CartSessionKey);

            if (string.IsNullOrWhiteSpace(cartJson))
            {
                return new ShoppingCart();
            }

            return JsonSerializer.Deserialize<ShoppingCart>(cartJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ShoppingCart();
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);

            if (existingItem == null)
            {
                cart.Items.Add(item);
            }
            else
            {
                existingItem.Quantity += 1;
            }

            SaveCart(cart);
        }

        public void RemoveFromCart(int itemId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(CartSessionKey);
        }

        private void SaveCart(ShoppingCart cart)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(
                CartSessionKey,
                JsonSerializer.Serialize(cart));
        }
    }
}
