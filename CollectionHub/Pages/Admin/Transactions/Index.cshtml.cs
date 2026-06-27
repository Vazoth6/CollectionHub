using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Pages.Admin.Transactions
{
    [Authorize(Roles = "Admin")]
    // <summary>
    // Representa o modelo de dados utilizado para o index model.
    // </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // <summary>
        // Obtém ou define transações.
        // </summary>
        public List<AdminTransactionDto> Transactions { get; set; } = new();
        // <summary>
        // Obtém ou define número total de páginas.
        // </summary>
        public int TotalPages { get; set; }
        // <summary>
        // Obtém ou define página atual.
        // </summary>
        public int CurrentPage { get; set; } = 1;
        private int PageSize = 20;

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define termo de pesquisa.
        // </summary>
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define data de início.
        // </summary>
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        // <summary>
        // Obtém ou define data de fim.
        // </summary>
        public DateTime? DateTo { get; set; }

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;

            var query = _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .AsQueryable();

            // Aplica filtros
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var search = SearchTerm.ToLower();
                query = query.Where(t =>
                    (t.Item != null && t.Item.Name.ToLower().Contains(search)) ||
                    (t.Buyer != null && t.Buyer.Name.ToLower().Contains(search)) ||
                    (t.Seller != null && t.Seller.Name.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(t => t.Status == Status);
            }

            if (DateFrom.HasValue)
            {
                var dateFrom = DateFrom.Value.Date;
                query = query.Where(t => t.Date >= dateFrom);
            }

            if (DateTo.HasValue)
            {
                var dateTo = DateTo.Value.Date.AddDays(1);
                query = query.Where(t => t.Date < dateTo);
            }

            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Preenche emails dos utilizadores numa consulta separada (evita problemas com null propagating)
            var userIds = transactions
                .Select(t => t.Buyer?.UserID)
                .Where(id => !string.IsNullOrEmpty(id))
                .Concat(transactions.Select(t => t.Seller?.UserID).Where(id => !string.IsNullOrEmpty(id)))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            Transactions = transactions.Select(t => new AdminTransactionDto
            {
                Id = t.Id,
                Date = t.Date,
                Price = t.Price,
                Status = t.Status,
                PaymentMethod = t.PaymentMethod,
                IsPaid = t.IsPaid,
                ShippingAddress = t.ShippingAddress,
                ItemId = t.ItemId,
                ItemName = t.Item != null ? t.Item.Name : "Item removido",
                BuyerName = t.Buyer != null ? t.Buyer.Name : "Utilizador removido",
                BuyerEmail = t.Buyer != null && users.ContainsKey(t.Buyer.UserID) ? users[t.Buyer.UserID] : "N/A",
                SellerName = t.Seller != null ? t.Seller.Name : "Utilizador removido",
                SellerEmail = t.Seller != null && users.ContainsKey(t.Seller.UserID) ? users[t.Seller.UserID] : "N/A"
            }).ToList();
        }

        // <summary>
        // Executa a operação de atualização de estado.
        // </summary>
        public async Task<IActionResult> OnPostUpdateStatusAsync(int transactionId, string newStatus)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Item)
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction == null)
                {
                    TempData["Error"] = "Transação não encontrada.";
                    return RedirectToPage();
                }

                var validStatuses = new[] { "Pendente", "Pago", "Enviado", "Entregue", "Cancelado" };
                if (!validStatuses.Contains(newStatus))
                {
                    TempData["Error"] = "Status inválido.";
                    return RedirectToPage();
                }

                transaction.Status = newStatus;

                if (newStatus == "Cancelado" && transaction.Item != null)
                {
                    transaction.Item.Status = "Disponível";
                }

                if (newStatus == "Entregue")
                {
                    transaction.Status = "Concluída";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin alterou status da transação {transactionId} para {newStatus}");
                TempData["Success"] = $"Status da transação #{transactionId} actualizado para '{newStatus}' com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao actualizar status: {ex.Message}");
                TempData["Error"] = "Erro ao actualizar o status da transação.";
            }

            return RedirectToPage();
        }
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para admin transaction dto.
    // </summary>
    public class AdminTransactionDto
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
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define método de pagamento.
        // </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        // <summary>
        // Indica se está pago.
        // </summary>
        public bool IsPaid { get; set; }
        // <summary>
        // Obtém ou define endereço de envio.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define item id.
        // </summary>
        public int ItemId { get; set; }
        // <summary>
        // Obtém ou define nome do item.
        // </summary>
        public string ItemName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do comprador.
        // </summary>
        public string BuyerName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define email do comprador.
        // </summary>
        public string BuyerEmail { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do vendedor.
        // </summary>
        public string SellerName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define email do vendedor.
        // </summary>
        public string SellerEmail { get; set; } = string.Empty;
    }
}
