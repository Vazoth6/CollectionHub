using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using CollectionHub.Data.Model.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CollectionHub.Pages.Shop
{
    [Authorize(Roles = "Vendedor,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CreateModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
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
                // Remove ImageUrl validation if no image is uploaded
                if (ImageFile == null || ImageFile.Length == 0)
                {
                    ModelState.Remove(nameof(CreateItem.ImageUrl));
                }

                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return Page();
                }

                // Process image if provided
                string? imageUrl = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    imageUrl = await SaveImageAsync(ImageFile);
                    if (imageUrl == null)
                    {
                        TempData["Error"] = "Erro ao processar a imagem.";
                        await LoadCategories();
                        return Page();
                    }
                }

                // Get authentication cookie
                var cookie = Request.Headers["Cookie"].ToString();

                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7102/";
                client.BaseAddress = new Uri(apiBaseUrl);

                if (!string.IsNullOrEmpty(cookie))
                {
                    client.DefaultRequestHeaders.Add("Cookie", cookie);
                }

                // Prepare data without ImageUrl if null
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

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validar tamanho (2MB)
                if (imageFile.Length > 2 * 1024 * 1024)
                {
                    return null;
                }

                // Validar tipo
                var validTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!validTypes.Contains(imageFile.ContentType))
                {
                    return null;
                }

                // Criar pasta se não existir
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Gerar nome único
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Guardar ficheiro
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"/images/items/{uniqueFileName}";
            }
            catch
            {
                return null;
            }
        }
    }
}