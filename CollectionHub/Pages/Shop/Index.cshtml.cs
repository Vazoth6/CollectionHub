using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Pages.Shop
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<ItemResponseDto> Items { get; set; } = new();
        public List<SelectListItem> CategoriesList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public List<string> SelectedCategories { get; set; } = new List<string>();

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        // Lista de categorias pré-definidas (para exibição no front-end)
        public List<CategoryDisplayDto> PredefinedCategories { get; set; } = new()
        {
            new CategoryDisplayDto { Name = "Vídeo-Jogos", Icon = "bi-controller" },
            new CategoryDisplayDto { Name = "Cartas Colecionáveis", Icon = "bi-suit-club" },
            new CategoryDisplayDto { Name = "Moedas", Icon = "bi-coin" },
            new CategoryDisplayDto { Name = "Selos", Icon = "bi-envelope-paper" },
            new CategoryDisplayDto { Name = "Action Figures", Icon = "bi-robot" },
            new CategoryDisplayDto { Name = "Livros", Icon = "bi-book" },
            new CategoryDisplayDto { Name = "Automóveis Miniatura", Icon = "bi-car-front" },
            new CategoryDisplayDto { Name = "Memorabilia", Icon = "bi-star" },
            new CategoryDisplayDto { Name = "Outros", Icon = "bi-box" }
        };

        public async Task OnGetAsync()
        {
            // Inicializar SelectedCategories se for null
            SelectedCategories ??= new List<string>();

            await LoadCategories();
            await LoadItems();
        }

        public async Task<IActionResult> OnPostBuyAsync(int itemId, string shippingAddress)
        {
            if (User.Identity.IsAuthenticated)
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7000/";
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
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Erro ao realizar compra. Tente novamente.";
                }

                return RedirectToPage();
            }

            return RedirectToPage("/Identity/Account/Login", new { returnUrl = "/Shop" });
        }

        private async Task LoadCategories()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7000/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var response = await client.GetAsync("api/CategoriesApi");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<CategoryResponseDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    CategoriesList = categories?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name} ({c.ItemCount})"
                    }).ToList() ?? new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar categorias: {ex.Message}");
                CategoriesList = new List<SelectListItem>();
            }
        }

        private async Task LoadItems()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7000/";
                client.BaseAddress = new Uri(apiBaseUrl);

                var query = new ItemListQueryDto
                {
                    SearchTerm = SearchTerm,
                    CategoryId = CategoryId,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    SortBy = SortBy ?? "name_asc",
                    Page = CurrentPage,
                    PageSize = 12
                };

                // Construir query string com suporte para múltiplas categorias
                var queryParameters = new List<string>();
                queryParameters.Add($"SearchTerm={Uri.EscapeDataString(query.SearchTerm ?? "")}");
                queryParameters.Add($"CategoryId={query.CategoryId}");
                queryParameters.Add($"MinPrice={query.MinPrice}");
                queryParameters.Add($"MaxPrice={query.MaxPrice}");
                queryParameters.Add($"SortBy={query.SortBy}");
                queryParameters.Add($"Page={query.Page}");
                queryParameters.Add($"PageSize={query.PageSize}");

                // Adicionar categorias selecionadas à query
                if (SelectedCategories != null && SelectedCategories.Any())
                {
                    foreach (var cat in SelectedCategories)
                    {
                        queryParameters.Add($"SelectedCategories={Uri.EscapeDataString(cat)}");
                    }
                }

                var queryString = "?" + string.Join("&", queryParameters);
                var response = await client.GetAsync($"api/ItemsApi{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Items = JsonSerializer.Deserialize<List<ItemResponseDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ItemResponseDto>();

                    // CORREÇÃO: Usar variáveis temporárias para out parameters
                    if (response.Headers.TryGetValues("X-Total-Count", out var totalCountValues))
                    {
                        if (int.TryParse(totalCountValues.FirstOrDefault(), out int totalItems))
                        {
                            TotalItems = totalItems;
                        }
                    }

                    if (response.Headers.TryGetValues("X-Total-Pages", out var totalPagesValues))
                    {
                        if (int.TryParse(totalPagesValues.FirstOrDefault(), out int totalPages))
                        {
                            TotalPages = totalPages;
                        }
                    }

                    if (TotalPages == 0 && TotalItems > 0)
                    {
                        TotalPages = (int)Math.Ceiling(TotalItems / 12.0);
                    }
                }
                else
                {
                    Items = new List<ItemResponseDto>();
                    TotalPages = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar itens: {ex.Message}");
                Items = new List<ItemResponseDto>();
                TotalPages = 1;
            }
        }
    }

    // DTO para exibição das categorias pré-definidas
    public class CategoryDisplayDto
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}