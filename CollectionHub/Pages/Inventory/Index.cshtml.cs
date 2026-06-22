using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

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
        public List<InventoryItemDto> ItemsPurchased { get; set; } = new();
        public List<InventoryItemDto> ItemsForSale { get; set; } = new();
        public List<InventoryTransactionDto> Transactions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = await GetCurrentMyUserId();
            if (userId == 0) return;

            var user = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == userId);
            WalletBalance = user?.WalletBalance ?? 0;

            // ⭐ ITENS COMPRADOS (via Transactions - Buyer)
            var purchasedItems = await _context.Transactions
                .Include(t => t.Item)
                .Where(t => t.BuyerId == userId && t.IsPaid)
                .Select(t => new InventoryItemDto
                {
                    Id = t.ItemId,
                    Name = t.Item != null ? t.Item.Name : "Item removido",
                    Description = t.Item != null ? t.Item.Description : string.Empty,
                    Price = t.Price,
                    ImageUrl = t.Item != null ? t.Item.ImageUrl : null,
                    Status = t.Status,
                    PurchaseDate = t.Date,
                    ShippingAddress = t.ShippingAddress
                })
                .ToListAsync();

            ItemsPurchased = purchasedItems;

            // ⭐ ITENS À VENDA (UserItems do utilizador)
            var forSaleItems = await _context.UserItems
                .Include(ui => ui.Item)
                .ThenInclude(i => i.Category)
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.Item)
                .Where(i => i.Status == "Disponível")
                .Select(i => new InventoryItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    ImageUrl = i.ImageUrl,
                    Status = i.Status,
                    SubmittedAt = i.SubmittedAt
                })
                .ToListAsync();

            ItemsForSale = forSaleItems;

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
                return 0;

            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (identityUser == null)
                return 0;

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