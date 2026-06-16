using CollectionHub.Data.Model.DTOs;
using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CollectionHub.Pages.Shop
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;

        public DetailsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ICartService cartService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cartService = cartService;
        }

        public ItemResponseDto? Item { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            Item = item;
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);

            if (item == null)
            {
                TempData["Error"] = "O item não foi encontrado.";
                return RedirectToPage("/Shop/Index");
            }

            if (!string.Equals(item.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Este item não está disponível para compra.";
                return RedirectToPage(new { id = itemId });
            }

            _cartService.AddToCart(new CartItem
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                ImageUrl = item.ImageUrl ?? string.Empty,
                Quantity = 1,
                SellerId = item.SellerId ?? 0,
                SellerName = item.SellerName ?? "N/A"
            });

            TempData["Success"] = "Item adicionado ao carrinho.";
            return RedirectToPage(new { id = itemId });
        }

        private async Task<ItemResponseDto?> GetItemAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var response = await client.GetAsync($"api/ItemsApi/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ItemResponseDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar item: {ex.Message}");
                return null;
            }
        }
    }
}
