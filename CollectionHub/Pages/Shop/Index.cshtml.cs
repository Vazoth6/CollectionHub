using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Pages.Shop
{
    // <summary>
    // Representa o modelo de dados utilizado pelo index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // <summary>
        // Obtém ou define artigos.
        // </summary>
        public List<ItemResponseDto> Items { get; set; } = new();
        // <summary>
        // Obtém ou define lista de categorias.
        // </summary>
        public List<SelectListItem> CategoriesList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define termo de pesquisa.
        // </summary>
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define id da categoria.
        // </summary>
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define preço mínimo.
        // </summary>
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define preço máximo.
        // </summary>
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define critério de ordenação.
        // </summary>
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define página atual.
        // </summary>
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define categorias selecionadas.
        // </summary>
        public List<string> SelectedCategories { get; set; } = new List<string>();

        // <summary>
        // Obtém ou define total de páginas.
        // </summary>
        public int TotalPages { get; set; }
        // <summary>
        // Obtém ou define total de artigos.
        // </summary>
        public int TotalItems { get; set; }

        // Lista de categorias carregadas da API (dinâmica)
        // <summary>
        // Obtém ou define categorias pré-definidas.
        // </summary>
        public List<CategoryDisplayDto> PredefinedCategories { get; set; } = new();

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync()
        {
            // Inicializa SelectedCategories se for null
            SelectedCategories ??= new List<string>();

            await LoadCategories();
            await LoadPredefinedCategories();  // Carrega categorias da API com ícones
            await LoadItems();
        }

        // <summary>
        // Executa a operação de compra.
        // </summary>
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

        // <summary>
        // Carrega as categorias da API e mapeia com os respetivos ícones
        // </summary>
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

                    // Mapeia categorias com ícones baseados no nome
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

        // <summary>
        // Retorna o ícone correspondente à categoria
        // </summary>
        private string GetIconForCategory(string categoryName)
        {
            return categoryName switch
            {
                string s when s.Contains("Pokémon", StringComparison.OrdinalIgnoreCase) => "bi-pokeball",
                string s when s.Contains("Yu-gi-oh", StringComparison.OrdinalIgnoreCase) => "bi-suit-diamond",
                string s when s.Contains("Invizimal", StringComparison.OrdinalIgnoreCase) => "bi-dragon",
                string s when s.Contains("Videojogo", StringComparison.OrdinalIgnoreCase) || s.Contains("Video", StringComparison.OrdinalIgnoreCase) => "bi-controller",
                string s when s.Contains("Tabuleiro", StringComparison.OrdinalIgnoreCase) || s.Contains("Board", StringComparison.OrdinalIgnoreCase) => "bi-grid",
                string s when s.Contains("Moeda", StringComparison.OrdinalIgnoreCase) || s.Contains("Coin", StringComparison.OrdinalIgnoreCase) => "bi-coin",
                string s when s.Contains("Carta", StringComparison.OrdinalIgnoreCase) || s.Contains("Card", StringComparison.OrdinalIgnoreCase) => "bi-suit-club",
                _ => "bi-box"
            };
        }

        // <summary>
        // Categorias de fallback em caso de erro na API
        // </summary>
        private List<CategoryDisplayDto> GetFallbackCategories()
        {
            return new List<CategoryDisplayDto>
            {
                new CategoryDisplayDto { Name = "Carta Pokémon", Icon = "bi-pokeball" },
                new CategoryDisplayDto { Name = "Carta Yu-gi-oh", Icon = "bi-suit-diamond" },
                new CategoryDisplayDto { Name = "Carta Invizimal", Icon = "bi-dragon" },
                new CategoryDisplayDto { Name = "Videojogo", Icon = "bi-controller" },
                new CategoryDisplayDto { Name = "Jogo de Tabuleiro", Icon = "bi-grid" },
                new CategoryDisplayDto { Name = "Moeda", Icon = "bi-coin" }
            };
        }

        private async Task LoadItems()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                // Constrói query string com suporte para múltiplas categorias
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

                // Adiciona categorias selecionadas à query (por nome)
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

                    // Obtém headers de paginação
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
                Console.WriteLine($"Erro ao carregar: {ex.Message}");
                Items = new List<ItemResponseDto>();
                TotalPages = 1;
                TotalItems = 0;
            }
        }
    }

    // DTO para exibição das categorias
    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para category display dto.
    // </summary>
    public class CategoryDisplayDto
    {
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define ícone.
        // </summary>
        public string Icon { get; set; } = string.Empty;
    }
}
