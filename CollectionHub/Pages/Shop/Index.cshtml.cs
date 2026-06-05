using CollectionHub.Data;
using CollectionHub.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Pages.Shop
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Item> Items { get; set; } = new List<Item>();

        public SelectList Categories { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        public async Task OnGetAsync()
        {
            Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                "Id",
                "Name");

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.UserItems)
                    .ThenInclude(ui => ui.User)
                .Where(i => i.Status == "Disponível")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(i =>
                    i.Name.Contains(Search) ||
                    i.Description.Contains(Search));
            }

            if (CategoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == CategoryId.Value);
            }

            if (MinPrice.HasValue)
            {
                query = query.Where(i => i.Price >= MinPrice.Value);
            }

            if (MaxPrice.HasValue)
            {
                query = query.Where(i => i.Price <= MaxPrice.Value);
            }

            Items = await query
                .OrderByDescending(i => i.SubmittedAt)
                .ToListAsync();
        }
    }
}
