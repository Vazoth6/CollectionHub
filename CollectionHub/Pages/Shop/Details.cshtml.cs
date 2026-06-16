using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Pages.Shop
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public DetailsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public ItemResponseDto? Item { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var response = await client.GetAsync($"api/ItemsApi/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Item = JsonSerializer.Deserialize<ItemResponseDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar item: {ex.Message}");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostBuyAsync(int itemId, string shippingAddress)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Identity/Account/Login", new { returnUrl = $"/Shop/Details/{itemId}" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var transactionData = new
                {
                    itemId = itemId,
                    shippingAddress = shippingAddress
                };

                var content = new StringContent(JsonSerializer.Serialize(transactionData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/TransactionsApi", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Compra realizada com sucesso!";
                    return RedirectToPage("/Shop");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Erro ao realizar compra. Tente novamente.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na compra: {ex.Message}");
                TempData["Error"] = "Erro ao conectar com o servidor. Tente novamente.";
            }

            return RedirectToPage(new { id = itemId });
        }
    }
}