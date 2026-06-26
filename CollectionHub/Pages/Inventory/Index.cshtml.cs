using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;

namespace CollectionHub.Pages.Inventory
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal WalletBalance { get; set; }
        public string ActiveProfile { get; set; } = "Comprador";
        public string ReturnUrl { get; set; } = "/Inventory";

        public bool IsBuyerProfile => ActiveProfile == "Comprador";
        public bool IsSellerProfile => ActiveProfile == "Vendedor";

        public List<InventoryItemDto> ItemsPurchased { get; set; } = new();
        public List<InventoryItemDto> ItemsForSale { get; set; } = new();
        public List<InventoryTransactionDto> Transactions { get; set; } = new();
        public int CompletedSalesCount { get; set; }

        public async Task OnGetAsync()
        {
            ReturnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

            var userId = await GetCurrentMyUserId();
            if (userId == 0) return;

            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            WalletBalance = user?.WalletBalance ?? 0;

            ActiveProfile = HttpContext.Session.GetString("ActiveProfile") ?? "Comprador";

            if (ActiveProfile != "Comprador" && ActiveProfile != "Vendedor")
            {
                ActiveProfile = "Comprador";
                HttpContext.Session.SetString("ActiveProfile", ActiveProfile);
            }

            await LoadUserData(userId);
        }

        public async Task<IActionResult> OnPostSellItemAsync(int itemId, decimal salePrice)
        {
            var userId = await GetCurrentMyUserId();

            if (userId == 0)
            {
                return Unauthorized();
            }

            if (salePrice <= 0)
            {
                TempData["Error"] = "O preço de venda tem de ser superior a zero.";
                return RedirectToPage();
            }

            var userItem = await _context.UserItems
                .Include(ui => ui.Item)
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

            if (userItem?.Item == null)
            {
                TempData["Error"] = "Este item não pertence à sua coleção.";
                return RedirectToPage();
            }

            if (userItem.Item.Status == "Disponível")
            {
                TempData["Error"] = "Este item já está à venda.";
                return RedirectToPage();
            }

            userItem.Item.Price = salePrice;
            userItem.Item.Status = "Disponível";
            userItem.Item.SubmittedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Item colocado à venda com sucesso.";
            return RedirectToPage();
        }

        private async Task LoadUserData(int userId)
        {
            // ⭐ ITENS NA COLEÇÃO (comprados e não disponíveis para venda)
            ItemsPurchased = await _context.UserItems
                .Include(ui => ui.Item)
                .ThenInclude(i => i!.Category)
                .Where(ui => ui.UserId == userId && ui.Item != null && ui.Item.Status != "Disponível")
                .Select(ui => new InventoryItemDto
                {
                    Id = ui.Item!.Id,
                    Name = ui.Item.Name,
                    Description = ui.Item.Description,
                    Price = ui.Item.Price,
                    ImageUrl = ui.Item.ImageUrl,
                    Status = ui.Item.Status,
                    SubmittedAt = ui.Item.SubmittedAt,
                    PurchaseDate = _context.Transactions
                        .Where(t => t.BuyerId == userId && t.ItemId == ui.Item.Id && t.IsPaid)
                        .OrderByDescending(t => t.Date)
                        .Select(t => (DateTime?)t.Date)
                        .FirstOrDefault(),
                    ShippingAddress = _context.Transactions
                        .Where(t => t.BuyerId == userId && t.ItemId == ui.Item.Id && t.IsPaid)
                        .OrderByDescending(t => t.Date)
                        .Select(t => t.ShippingAddress)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // ⭐ ITENS À VENDA
            ItemsForSale = await _context.UserItems
                .Include(ui => ui.Item)
                .Where(ui => ui.UserId == userId && ui.Item != null && ui.Item.Status == "Disponível")
                .Select(ui => new InventoryItemDto
                {
                    Id = ui.Item!.Id,
                    Name = ui.Item.Name,
                    Description = ui.Item.Description,
                    Price = ui.Item.Price,
                    ImageUrl = ui.Item.ImageUrl,
                    Status = ui.Item.Status,
                    SubmittedAt = ui.Item.SubmittedAt
                })
                .ToListAsync();

            // ⭐ VENDAS REALIZADAS (transações concluídas como vendedor)
            CompletedSalesCount = await _context.Transactions
                .CountAsync(t => t.SellerId == userId && t.IsPaid);

            // ⭐ TRANSAÇÕES (Compras + Vendas)
            var purchases = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == userId)
                .OrderByDescending(t => t.Date)
                .Select(t => new InventoryTransactionDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Price = t.Price,
                    ShippingAddress = t.ShippingAddress,
                    Status = t.Status,
                    ItemName = t.Item != null ? t.Item.Name : "Item removido",
                    OtherPartyName = t.Seller != null ? t.Seller.Name : "Vendedor desconhecido",
                    TransactionType = "Compra"
                })
                .ToListAsync();

            var sales = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Buyer)
                .Where(t => t.SellerId == userId)
                .OrderByDescending(t => t.Date)
                .Select(t => new InventoryTransactionDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Price = t.Price,
                    ShippingAddress = t.ShippingAddress,
                    Status = t.Status,
                    ItemName = t.Item != null ? t.Item.Name : "Item removido",
                    OtherPartyName = t.Buyer != null ? t.Buyer.Name : "Comprador desconhecido",
                    TransactionType = "Venda"
                })
                .ToListAsync();

            Transactions = purchases.Concat(sales)
                .OrderByDescending(t => t.Date)
                .ToList();
        }

        private async Task<int> GetCurrentMyUserId()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                return 0;
            }

            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (identityUser == null)
            {
                return 0;
            }

            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.UserID == identityUser.Id);

            return myUser?.Id ?? 0;
        }
    }

    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? ShippingAddress { get; set; }
    }

    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string OtherPartyName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
    }
}