using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Pages.Shop
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [BindProperty]
        public CreateItemDto CreateItem { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> CategoriesSelectList { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadCategories();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("=== ONPOSTASYNC START ===");

                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return Page();
                }

                // ⭐ PROCESSAR IMAGEM
                string? imageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    _logger.LogInformation($"A processar imagem: {ImageFile.FileName}, {ImageFile.Length} bytes");

                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "A imagem é muito grande. Máximo 5MB.";
                        await LoadCategories();
                        return Page();
                    }

                    var validTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!validTypes.Contains(ImageFile.ContentType))
                    {
                        TempData["Error"] = "Formato de imagem não suportado.";
                        await LoadCategories();
                        return Page();
                    }

                    imageUrl = await SaveImageAsync(ImageFile);
                    if (imageUrl == null)
                    {
                        TempData["Error"] = "Erro ao guardar a imagem.";
                        await LoadCategories();
                        return Page();
                    }
                }

                // ⭐ ENVIAR PARA API
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
                    ImageUrl = imageUrl
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
                _logger.LogError($"EXCEÇÃO: {ex.Message}");
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
                new CategoryResponseDto { Id = 1, Name = "Carta Pokémon" },
                new CategoryResponseDto { Id = 2, Name = "Carta Yu-gi-oh" },
                new CategoryResponseDto { Id = 3, Name = "Carta Invizimal" },
                new CategoryResponseDto { Id = 4, Name = "Videojogo" },
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

        // ⭐ MÉTODO DE UPLOAD - VERSÃO DEFINITIVA
        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                // Criar pasta se não existir
                var webRootPath = _webHostEnvironment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRootPath, "images", "items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Gerar nome único
                var uniqueFileName = $"{Guid.NewGuid():N}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return $"/images/items/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro SaveImageAsync: {ex.Message}");
                return null;
            }
        }
    }
}