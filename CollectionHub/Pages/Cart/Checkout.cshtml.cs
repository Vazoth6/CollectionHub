using CollectionHub.Data.Model.DTOs;
using CollectionHub.Models;
using CollectionHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace CollectionHub.Pages.Cart
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private const string CheckoutShippingAddressSessionKey = "CheckoutShippingAddress";

        private readonly ICartService _cartService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CheckoutModel(
            ICartService cartService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _cartService = cartService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public CollectionHub.Models.Cart Cart { get; set; } = new();

        [BindProperty]
        public string ShippingAddress { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentMethod { get; set; } = "MBWay";

        public int CurrentStep { get; set; } = 1;

        public IActionResult OnGet(int step = 1)
        {
            Cart = _cartService.GetCart();

            if (!Cart.Items.Any() && step != 3)
            {
                TempData["Error"] = "O seu carrinho está vazio.";
                return RedirectToPage("/Cart/Index");
            }

            CurrentStep = step;
            ShippingAddress = HttpContext.Session.GetString(CheckoutShippingAddressSessionKey) ?? string.Empty;
            return Page();
        }

        public IActionResult OnPostNext()
        {
            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio.";
                return RedirectToPage("/Cart/Index");
            }

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                ModelState.AddModelError(nameof(ShippingAddress), "O endereço de entrega é obrigatório.");
                CurrentStep = 1;
                return Page();
            }

            HttpContext.Session.SetString(CheckoutShippingAddressSessionKey, ShippingAddress.Trim());
            return RedirectToPage(new { step = 2 });
        }

        public async Task<IActionResult> OnPostPaymentAsync()
        {
            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio.";
                return RedirectToPage("/Cart/Index");
            }

            ShippingAddress = HttpContext.Session.GetString(CheckoutShippingAddressSessionKey) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                TempData["Error"] = "Indique o endereço de entrega.";
                return RedirectToPage(new { step = 1 });
            }

            if (string.IsNullOrWhiteSpace(PaymentMethod))
            {
                TempData["Error"] = "Escolha um método de pagamento.";
                return RedirectToPage(new { step = 2 });
            }

            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
            client.BaseAddress = new Uri(apiBaseUrl);

            var allSuccessful = true;
            var errors = new List<string>();

            foreach (var item in Cart.Items)
            {
                var transactionData = new CreateTransactionDto
                {
                    ItemId = item.Id,
                    ShippingAddress = ShippingAddress
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(transactionData),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("api/TransactionsApi", content);

                if (!response.IsSuccessStatusCode)
                {
                    allSuccessful = false;
                    var error = await response.Content.ReadAsStringAsync();
                    errors.Add($"Erro ao processar '{item.Name}': {error}");
                }
            }

            if (!allSuccessful)
            {
                TempData["Error"] = string.Join(" ", errors);
                return RedirectToPage(new { step = 2 });
            }

            _cartService.ClearCart();
            HttpContext.Session.Remove(CheckoutShippingAddressSessionKey);
            TempData["Success"] = "Compra realizada com sucesso! Obrigado pela sua compra.";
            return RedirectToPage(new { step = 3 });
        }
    }
}
