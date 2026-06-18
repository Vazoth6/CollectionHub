using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Pages.Shop
{
    [Authorize(Roles = "Vendedor,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public CreateItemDto CreateItem { get; set; } = new();

        public List<SelectListItem> CategoriesSelectList { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadCategories();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return Page();
                }

                var cookie = Request.Headers["Cookie"].ToString();
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                if (!string.IsNullOrEmpty(cookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie);
                }

                var itemData = new
                {
                    CreateItem.Name,
                    CreateItem.Description,
                    CreateItem.Price,
                    CreateItem.CategoryId,
                    CreateItem.ImageUrl
                };

                var json = JsonSerializer.Serialize(itemData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/ItemsApi", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Item '{CreateItem.Name}' publicado com sucesso!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    TempData["Error"] = $"Erro ao publicar item: {responseContent}";
                    await LoadCategories();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro: {ex.Message}");
                TempData["Error"] = $"Erro: {ex.Message}";
                await LoadCategories();
                return Page();
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var response = await client.GetAsync("api/CategoriesApi");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryResponseDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    CategoriesSelectList = categories?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    await LoadCategoriesFallback();
                }
            }
            catch
            {
                await LoadCategoriesFallback();
            }
        }

        private Task LoadCategoriesFallback()
        {
            var categories = new List<CategoryResponseDto>
            {
                new CategoryResponseDto { Id = 1, Name = "Carta Pokemon" },
                new CategoryResponseDto { Id = 2, Name = "Carta Yu-gi-oh" },
                new CategoryResponseDto { Id = 3, Name = "Carta Invizimal" },
                new CategoryResponseDto { Id = 4, Name = "Vídeo-jogo" },
                new CategoryResponseDto { Id = 5, Name = "Jogo de Tabuleiro" },
                new CategoryResponseDto { Id = 6, Name = "Moeda" }
            };

            CategoriesSelectList = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return Task.CompletedTask;
        }
    }
}