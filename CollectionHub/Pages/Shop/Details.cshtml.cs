using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Data.Model.DTOs;
using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CollectionHub.Pages.Shop
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;

        public DetailsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ICartService cartService,
            ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cartService = cartService;
            _context = context;
        }

        public ItemResponseDto? Item { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            Item = item;
            await LoadLikeInfoAsync(id);

            var dbItem = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            SubmittedAt = dbItem?.SubmittedAt;

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int itemId)
        {
            var item = await GetItemAsync(itemId);

            if (item == null)
            {
                TempData["Error"] = "Não foi encontrado o coleccionável.";
                return RedirectToPage("/Shop/Index");
            }

            if (!string.Equals(item.Status, "Disponível", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Este coleccionável não está disponível para compra.";
                return RedirectToPage(new { id = itemId });
            }

            var currentUser = await GetCurrentMyUserAsync();

            if (currentUser != null && item.SellerId == currentUser.Id)
            {
                TempData["Error"] = "Não pode adicionar o seu próprio item ao carrinho.";
                return RedirectToPage(new { id = itemId });
            }

            var cart = _cartService.GetCart();

            if (cart.Items.Any(i => i.Id == item.Id))
            {
                TempData["Error"] = "Este coleccionável já se encontra no carrinho.";
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

            TempData["Success"] = "Coleccionável adicionado ao carrinho.";
            return RedirectToPage(new { id = itemId });
        }

        public async Task<IActionResult> OnPostToggleLikeAsync(int itemId)
        {
            var currentUser = await GetCurrentMyUserAsync();

            if (currentUser == null)
            {
                TempData["Error"] = "Inicie sessão para gostar de artigos.";
                return RedirectToPage(new { id = itemId });
            }

            var itemExists = await _context.Items.AnyAsync(i => i.Id == itemId);

            if (!itemExists)
            {
                TempData["Error"] = "O coleccionável não foi encontrado.";
                return RedirectToPage("/Shop/Index");
            }

            var existingLike = await _context.ItemLikes
                .FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == currentUser.Id);

            if (existingLike == null)
            {
                _context.ItemLikes.Add(new ItemLike
                {
                    ItemId = itemId,
                    UserId = currentUser.Id
                });

                TempData["Success"] = "Gosto adicionado.";
            }
            else
            {
                _context.ItemLikes.Remove(existingLike);
                TempData["Success"] = "Gosto removido.";
            }

            await _context.SaveChangesAsync();

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
                Console.WriteLine($"Erro ao carregar: {ex.Message}");
                return null;
            }
        }

        private async Task LoadLikeInfoAsync(int itemId)
        {
            LikeCount = await _context.ItemLikes.CountAsync(l => l.ItemId == itemId);

            var currentUser = await GetCurrentMyUserAsync();

            if (currentUser == null)
            {
                IsLikedByCurrentUser = false;
                return;
            }

            IsLikedByCurrentUser = await _context.ItemLikes
                .AnyAsync(l => l.ItemId == itemId && l.UserId == currentUser.Id);
        }

        private async Task<MyUser?> GetCurrentMyUserAsync()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (identityUser == null)
            {
                return null;
            }

            return await _context.MyUsers
                .FirstOrDefaultAsync(m => m.UserID == identityUser.Id);
        }
    }
}
