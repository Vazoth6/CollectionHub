using System.Text.Json;
using CollectionHub.Models;

namespace CollectionHub.Services
{
    // <summary>
    // Serviço de domínio responsável por operações de carrinho, como adicionar, remover e calcular totais.
    // </summary>
    public class CartService : ICartService
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // <summary>
        // Obtém carrinho
        // </summary>
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

        // <summary>
        // Adiciona item ao carrinho. Se o item já estiver presente, apenas incrementa a quantidade
        // </summary>
        public void AddToCart(CartItem item)
        {
            var cart = GetCart();

            var existingItem = cart.Items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                // O mesmo colecionável não deve aparecer duplicado no carrinho; incrementamos apenas a quantidade.
                existingItem.Quantity += 1;
                SaveCart(cart);
                return;
            }

            item.Quantity = Math.Max(1, item.Quantity);
            cart.Items.Add(item);
            SaveCart(cart);
        }

        // <summary>
        // Actualiza a quantidade de um item no carrinho com base nos dados recebidos.
        // </summary>
        public void UpdateQuantity(int itemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
            {
                return;
            }

            // Quantidade zero funciona como remoção para simplificar os formulários e a API.
            if (quantity <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            SaveCart(cart);
        }

        // <summary>
        // Remove item do carrinho.
        // </summary>
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

        // <summary>
        // Executa a operação de limpeza do carrinho.
        // </summary>
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
