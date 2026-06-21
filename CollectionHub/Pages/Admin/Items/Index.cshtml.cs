using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Pages.Admin.Items
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<AdminItemDto> Items { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;
        private int PageSize = 15;

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

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

    public class AdminItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}