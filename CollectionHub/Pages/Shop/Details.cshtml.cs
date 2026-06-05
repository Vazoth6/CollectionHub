using CollectionHub.Data;
using CollectionHub.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Pages.Shop
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Item Item { get; set; } = default!;

        public string SellerName { get; set; } = "Vendedor desconhecido";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.UserItems)
                    .ThenInclude(ui => ui.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            Item = item;
            SellerName = item.UserItems.FirstOrDefault()?.User?.Name ?? "Vendedor desconhecido";

            return Page();
        }
    }
}
