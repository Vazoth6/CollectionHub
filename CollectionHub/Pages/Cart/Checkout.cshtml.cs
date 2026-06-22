using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CollectionHub.Models;
using CollectionHub.Services;
using CollectionHub.Data.Model.DTOs;
using CollectionHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Pages.Cart
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public CheckoutModel(
            ICartService cartService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
        }

        public ShoppingCart Cart { get; set; } = new();
        public decimal WalletBalance { get; set; }

        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentMethod { get; set; } = "Carteira Virtual";

        public int CurrentStep { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int step = 1)
        {
            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            // Obter saldo da carteira
            var userId = await GetCurrentMyUserId();
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            WalletBalance = user?.WalletBalance ?? 0;

            CurrentStep = step;
            return Page();
        }

        public IActionResult OnPostNext(string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                ModelState.AddModelError(nameof(ShippingAddress), "O endereço de entrega é obrigatório.");
                CurrentStep = 1;
                Cart = _cartService.GetCart();
                return Page();
            }

            ShippingAddress = shippingAddress;
            return RedirectToPage(new { step = 2 });
        }

        public async Task<IActionResult> OnPostPaymentAsync(string paymentMethod)
        {
            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio.";
                return RedirectToPage("/Cart/Index");
            }

            PaymentMethod = paymentMethod;
            ShippingAddress = TempData["ShippingAddress"] as string ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                return RedirectToPage(new { step = 1 });
            }

            // Obter utilizador atual
            var userId = await GetCurrentMyUserId();
            var buyer = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (buyer == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToPage("/Cart/Index");
            }

            // Verificar saldo
            var totalAmount = Cart.Total;
            if (buyer.WalletBalance < totalAmount)
            {
                TempData["Error"] = $"Saldo insuficiente. Tem {buyer.WalletBalance:C} e precisa de {totalAmount:C}.";
                return RedirectToPage(new { step = 2 });
            }

            // ⭐ PROCESSAR COMPRA
            bool allSuccessful = true;
            var errors = new List<string>();

            foreach (var cartItem in Cart.Items)
            {
                try
                {
                    // Usar a API para comprar
                    var client = _httpClientFactory.CreateClient();
                    var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                    client.BaseAddress = new Uri(apiBaseUrl);

                    var requestData = new
                    {
                        shippingAddress = ShippingAddress
                    };

                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"api/ItemsApi/Buy/{cartItem.Id}", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        errors.Add($"Erro ao comprar '{cartItem.Name}': {errorContent}");
                        allSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Erro ao comprar '{cartItem.Name}': {ex.Message}");
                    allSuccessful = false;
                }
            }

            if (allSuccessful)
            {
                _cartService.ClearCart();
                TempData["Success"] = "Compra realizada com sucesso! Os itens foram adicionados ao seu inventário.";
                return RedirectToPage(new { step = 3 });
            }
            else
            {
                TempData["Error"] = string.Join(" | ", errors);
                return RedirectToPage(new { step = 2 });
            }
        }

        private async Task<int> GetCurrentMyUserId()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
                return 0;

            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (identityUser == null)
                return 0;

            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.UserID == identityUser.Id);

            return myUser?.Id ?? 0;
        }
    }
}