using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Pages.Admin.Items
{
    [Authorize(Roles = "Admin")]
    // <summary>
    // Representa o modelo de dados utilizado para o index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // <summary>
        // Obtém ou define items.
        // </summary>
        public List<AdminItemDto> Items { get; set; } = new();
        // <summary>
        // Obtém ou define categorias.
        // </summary>
        public List<Category> Categories { get; set; } = new();
        // <summary>
        // Obtém ou define total de páginas.
        // </summary>
        public int TotalPages { get; set; }
        // <summary>
        // Obtém ou define página atual.
        // </summary>
        public int CurrentPage { get; set; } = 1;
        private int PageSize = 15;

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

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.UserItems)
                .ThenInclude(ui => ui.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(i => i.Name.Contains(SearchTerm));
            }

            if (CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == CategoryId.Value);
            }

            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var items = await query
                .OrderByDescending(i => i.SubmittedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Items = items.Select(i => new AdminItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Price = i.Price,
                Status = i.Status,
                ImageUrl = i.ImageUrl,
                CategoryName = i.Category?.Name ?? "Sem Categoria",
                SellerName = i.UserItems.FirstOrDefault()?.User?.Name ?? "Vendedor Desconhecido",
                SubmittedAt = i.SubmittedAt
            }).ToList();

            Categories = await _context.Categories.ToListAsync();
        }
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para admin item dto.
    // </summary>
    public class AdminItemDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define endereço da imagem.
        // </summary>
        public string? ImageUrl { get; set; }
        // <summary>
        // Obtém ou define nome da categoria.
        // </summary>
        public string CategoryName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do vendedor.
        // </summary>
        public string SellerName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define data de submissão.
        // </summary>
        public DateTime SubmittedAt { get; set; }
    }
}
