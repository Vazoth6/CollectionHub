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

        // Lista de categorias carregadas da API (dinâmica)
        public List<CategoryDisplayDto> PredefinedCategories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Inicializar SelectedCategories se for null
            SelectedCategories ??= new List<string>();

            await LoadCategories();
            await LoadPredefinedCategories();  // Carregar categorias da API com ícones
            await LoadItems();
        }

        public async Task<IActionResult> OnPostBuyAsync(int itemId, string shippingAddress)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Identity/Account/Login", new { returnUrl = "/Shop" });
            }

            try
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na compra: {ex.Message}");
                TempData["Error"] = "Erro ao conectar com o servidor. Tente novamente.";
            }

            return RedirectToPage();
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

        /// <summary>
        /// Carrega as categorias da API e mapeia com os respetivos ícones
        /// </summary>
        private async Task LoadPredefinedCategories()
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

                    // Mapear categorias com ícones baseados no nome
                    PredefinedCategories = categories?.Select(c => new CategoryDisplayDto
                    {
                        Name = c.Name,
                        Icon = GetIconForCategory(c.Name)
                    }).ToList() ?? new List<CategoryDisplayDto>();
                }
                else
                {
                    // Fallback em caso de erro na API
                    PredefinedCategories = GetFallbackCategories();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar categorias pré-definidas: {ex.Message}");
                // Fallback para categorias hardcoded em caso de erro
                PredefinedCategories = GetFallbackCategories();
            }
        }

        /// <summary>
        /// Retorna o ícone correspondente à categoria
        /// </summary>
        private string GetIconForCategory(string categoryName)
        {
            return categoryName switch
            {
                string s when s.Contains("Pokemon", StringComparison.OrdinalIgnoreCase) => "bi-pokeball",
                string s when s.Contains("Yu-gi-oh", StringComparison.OrdinalIgnoreCase) => "bi-suit-diamond",
                string s when s.Contains("Invizimal", StringComparison.OrdinalIgnoreCase) => "bi-dragon",
                string s when s.Contains("Vídeo", StringComparison.OrdinalIgnoreCase) || s.Contains("Video", StringComparison.OrdinalIgnoreCase) => "bi-controller",
                string s when s.Contains("Tabuleiro", StringComparison.OrdinalIgnoreCase) || s.Contains("Board", StringComparison.OrdinalIgnoreCase) => "bi-grid",
                string s when s.Contains("Moeda", StringComparison.OrdinalIgnoreCase) || s.Contains("Coin", StringComparison.OrdinalIgnoreCase) => "bi-coin",
                string s when s.Contains("Carta", StringComparison.OrdinalIgnoreCase) || s.Contains("Card", StringComparison.OrdinalIgnoreCase) => "bi-suit-club",
                _ => "bi-box"
            };
        }

        /// <summary>
        /// Categorias de fallback em caso de erro na API
        /// </summary>
        private List<CategoryDisplayDto> GetFallbackCategories()
        {
            return new List<CategoryDisplayDto>
            {
                new CategoryDisplayDto { Name = "Carta Pokemon", Icon = "bi-pokeball" },
                new CategoryDisplayDto { Name = "Carta Yu-gi-oh", Icon = "bi-suit-diamond" },
                new CategoryDisplayDto { Name = "Carta Invizimal", Icon = "bi-dragon" },
                new CategoryDisplayDto { Name = "Vídeo-jogo", Icon = "bi-controller" },
                new CategoryDisplayDto { Name = "Jogo de Tabuleiro", Icon = "bi-grid" },
                new CategoryDisplayDto { Name = "Moeda", Icon = "bi-coin" }
            };
        }

        private async Task LoadItems()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7000/";
                client.BaseAddress = new Uri(apiBaseUrl);

                // Construir query string com suporte para múltiplas categorias
                var queryParameters = new List<string>();

                if (!string.IsNullOrEmpty(SearchTerm))
                    queryParameters.Add($"SearchTerm={Uri.EscapeDataString(SearchTerm)}");

                if (CategoryId.HasValue && CategoryId.Value > 0)
                    queryParameters.Add($"CategoryId={CategoryId}");

                if (MinPrice.HasValue)
                    queryParameters.Add($"MinPrice={MinPrice}");

                if (MaxPrice.HasValue)
                    queryParameters.Add($"MaxPrice={MaxPrice}");

                if (!string.IsNullOrEmpty(SortBy))
                    queryParameters.Add($"SortBy={SortBy}");

                queryParameters.Add($"Page={CurrentPage}");
                queryParameters.Add($"PageSize=12");

                // Adicionar categorias selecionadas à query (por nome)
                if (SelectedCategories != null && SelectedCategories.Any())
                {
                    foreach (var cat in SelectedCategories)
                    {
                        queryParameters.Add($"SelectedCategories={Uri.EscapeDataString(cat)}");
                    }
                }

                var queryString = queryParameters.Any() ? "?" + string.Join("&", queryParameters) : "";
                var response = await client.GetAsync($"api/ItemsApi{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Items = JsonSerializer.Deserialize<List<ItemResponseDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ItemResponseDto>();

                    // Obter headers de paginação
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

                    if (TotalPages == 0) TotalPages = 1;
                }
                else
                {
                    Items = new List<ItemResponseDto>();
                    TotalPages = 1;
                    TotalItems = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar itens: {ex.Message}");
                Items = new List<ItemResponseDto>();
                TotalPages = 1;
                TotalItems = 0;
            }
        }
    }

    // DTO para exibição das categorias
    public class CategoryDisplayDto
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}