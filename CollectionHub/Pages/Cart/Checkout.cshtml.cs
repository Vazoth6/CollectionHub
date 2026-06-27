using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CollectionHub.Models;
using CollectionHub.Services;
using CollectionHub.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Pages.Cart
{
    [Authorize]
    // <summary>
    // Representa o modelo de dados utilizado para o checkout model.
    // </summary>
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutModel> _logger;

        public CheckoutModel(
            ICartService cartService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<CheckoutModel> logger)
        {
            _cartService = cartService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        // <summary>
        // Obtém ou define carrinho.
        // </summary>
        public ShoppingCart Cart { get; set; } = new();
        // <summary>
        // Obtém ou define saldo da carteira.
        // </summary>
        public decimal WalletBalance { get; set; }

        [BindProperty]
        // <summary>
        // Obtém ou define endereço de entrega.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;

        [BindProperty]
        // <summary>
        // Obtém ou define método de pagamento.
        // </summary>
        public string PaymentMethod { get; set; } = "Carteira Virtual";

        // <summary>
        // Obtém ou define passo atual.
        // </summary>
        public int CurrentStep { get; set; } = 1;

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task<IActionResult> OnGetAsync(int step = 1)
        {
            _logger.LogInformation($"=== ONGET STEP {step} ===");

            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            var userId = await GetCurrentMyUserId();
            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            WalletBalance = user?.WalletBalance ?? 0;

            // RECUPERA ENDEREÇO DO TempData
            if (TempData.ContainsKey("ShippingAddress"))
            {
                ShippingAddress = TempData["ShippingAddress"] as string ?? string.Empty;
                // MANTÉM O TempData PARA O PRÓXIMO REDIRECIONAMENTO
                TempData.Keep("ShippingAddress");
                _logger.LogInformation($"ShippingAddress recuperado do TempData: {ShippingAddress}");
            }

            CurrentStep = step;
            return Page();
        }

        // <summary>
        // Executa a operação de avanço.
        // </summary>
        public IActionResult OnPostNext(string shippingAddress)
        {
            _logger.LogInformation($"=== ONPOSTNEXT - Endereço: '{shippingAddress}' ===");

            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                ModelState.AddModelError(nameof(ShippingAddress), "O endereço de entrega é obrigatório.");
                CurrentStep = 1;
                Cart = _cartService.GetCart();
                return Page();
            }

            // GUARDA NO TempData
            TempData["ShippingAddress"] = shippingAddress;
            // FORÇA A MANTER O TempData
            TempData.Keep("ShippingAddress");
            _logger.LogInformation($"ShippingAddress guardado no TempData: {shippingAddress}");

            return RedirectToPage(new { step = 2 });
        }

        // <summary>
        // Executa a operação de pagamento.
        // </summary>
        public async Task<IActionResult> OnPostPaymentAsync(string paymentMethod)
        {
            _logger.LogInformation("=== ONPOSTPAYMENT ===");
            _logger.LogInformation($"PaymentMethod: {paymentMethod}");

            Cart = _cartService.GetCart();

            if (!Cart.Items.Any())
            {
                TempData["Error"] = "O seu carrinho está vazio.";
                return RedirectToPage("/Cart/Index");
            }

            PaymentMethod = paymentMethod;

            ShippingAddress = TempData["ShippingAddress"] as string ?? string.Empty;
            _logger.LogInformation($"ShippingAddress recuperado: '{ShippingAddress}'");

            if (string.IsNullOrWhiteSpace(ShippingAddress))
            {
                _logger.LogWarning("Endereço vazio! Redirecionando para o passo 1.");
                TempData["Error"] = "Endereço de entrega não fornecido. Por favor, preencha novamente.";
                return RedirectToPage(new { step = 1 });
            }

            var userId = await GetCurrentMyUserId();
            var buyer = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (buyer == null)
            {
                TempData["Error"] = "Utilizador não encontrado.";
                return RedirectToPage("/Cart/Index");
            }

            var totalAmount = Cart.Total;
            _logger.LogInformation($"Total: {totalAmount}, Saldo do comprador: {buyer.WalletBalance}");

            if (buyer.WalletBalance < totalAmount)
            {
                TempData["Error"] = $"Saldo insuficiente. Tem {buyer.WalletBalance:C} e precisa de {totalAmount:C}.";
                TempData["ShippingAddress"] = ShippingAddress;
                TempData.Keep("ShippingAddress");
                return RedirectToPage(new { step = 2 });
            }

            // OBTÉM O COOKIE DE AUTENTICAÇÃO DA REQUISIÇÃO ATUAL
            var cookie = Request.Headers["Cookie"].ToString();
            _logger.LogInformation($"Cookie presente: {!string.IsNullOrEmpty(cookie)}");

            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
            client.BaseAddress = new Uri(apiBaseUrl);

            // ADICIONA O COOKIE AO HTTPCLIENT
            if (!string.IsNullOrEmpty(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
                _logger.LogInformation("Cookie adicionado ao HttpClient");
            }

            bool allSuccessful = true;
            var errors = new List<string>();

            foreach (var cartItem in Cart.Items)
            {
                try
                {
                    _logger.LogInformation($"A comprar item: {cartItem.Id} - {cartItem.Name}");

                    var requestData = new
                    {
                        shippingAddress = ShippingAddress
                    };

                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"api/ItemsApi/Buy/{cartItem.Id}", content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Status: {response.StatusCode}");
                    _logger.LogInformation($"Resposta: {responseContent}");

                    if (!response.IsSuccessStatusCode)
                    {
                        errors.Add($"Erro ao comprar '{cartItem.Name}': {responseContent}");
                        allSuccessful = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro: {ex.Message}");
                    errors.Add($"Erro ao comprar '{cartItem.Name}': {ex.Message}");
                    allSuccessful = false;
                }
            }

            if (allSuccessful)
            {
                _cartService.ClearCart();
                TempData.Remove("ShippingAddress");
                TempData["Success"] = "Compra realizada com sucesso! Os itens foram adicionados ao seu inventário.";
                return RedirectToPage(new { step = 3 });
            }
            else
            {
                TempData["Error"] = string.Join(" | ", errors);
                TempData["ShippingAddress"] = ShippingAddress;
                TempData.Keep("ShippingAddress");
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
