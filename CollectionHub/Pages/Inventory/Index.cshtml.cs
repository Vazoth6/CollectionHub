using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;

namespace CollectionHub.Pages.Inventory
{
    [Authorize]
    // <summary>
    // Representa o modelo de dados utilizado pelo index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // <summary>
        // Obtém ou define o saldo da carteira.
        // </summary>
        public decimal WalletBalance { get; set; }
        // <summary>
        // Obtém ou define o perfil ativo.
        // </summary>
        public string ActiveProfile { get; set; } = "Comprador";
        // <summary>
        // Obtém ou define URL de retorno.
        // </summary>
        public string ReturnUrl { get; set; } = "/Inventory";

        public bool IsBuyerProfile => ActiveProfile == "Comprador";
        public bool IsSellerProfile => ActiveProfile == "Vendedor";

        // <summary>
        // Obtém ou define items comprados.
        // </summary>
        public List<InventoryItemDto> ItemsPurchased { get; set; } = new();
        // <summary>
        // Obtém ou define items à venda.
        // </summary>
        public List<InventoryItemDto> ItemsForSale { get; set; } = new();
        // <summary>
        // Obtém ou define transações.
        // </summary>
        public List<InventoryTransactionDto> Transactions { get; set; } = new();
        // <summary>
        // Obtém ou define vendas concluídas (como vendedor).
        // </summary>
        public int CompletedSalesCount { get; set; }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
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

        // <summary>
        // Executa a operação de colocar item à venda.
        // </summary>
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
            // ITEMS NA COLEÇÃO (comprados e não disponíveis para venda)
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

            // ITEMS À VENDA
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

            // VENDAS REALIZADAS (transações concluídas como vendedor)
            CompletedSalesCount = await _context.Transactions
                .CountAsync(t => t.SellerId == userId && t.IsPaid);

            // TRANSAÇÕES (Compras + Vendas)
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

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para inventory item dto.
    // </summary>
    public class InventoryItemDto
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
        // Obtém ou define descrição.
        // </summary>
        public string Description { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define endereço da imagem.
        // </summary>
        public string? ImageUrl { get; set; }
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define data de compra.
        // </summary>
        public DateTime? PurchaseDate { get; set; }
        // <summary>
        // Obtém ou define data de submissão.
        // </summary>
        public DateTime SubmittedAt { get; set; }
        // <summary>
        // Obtém ou define endereço de envio.
        // </summary>
        public string? ShippingAddress { get; set; }
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para inventory transaction dto.
    // </summary>
    public class InventoryTransactionDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define data.
        // </summary>
        public DateTime Date { get; set; }
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define endereço de envio.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do item.
        // </summary>
        public string ItemName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome da outra parte.
        // </summary>
        public string OtherPartyName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define tipo de transação.
        // </summary>
        public string TransactionType { get; set; } = string.Empty;
    }
}
