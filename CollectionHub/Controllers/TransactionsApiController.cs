using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Data.Model.DTOs;

namespace CollectionHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/TransactionsApi
        /// Obtém todas as transações do utilizador autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<TransactionListResponseDto>> GetMyTransactions()
        {
            var myUserId = await GetCurrentMyUserId();
            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var purchases = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == myUserId)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionListItemDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Price = t.Price,
                    ShippingAddress = t.ShippingAddress,
                    Status = t.Status,
                    ItemName = t.Item != null ? t.Item.Name : "Item não disponível",
                    OtherPartyName = t.Seller != null ? t.Seller.Name : "Vendedor desconhecido",
                    TransactionType = "Compra"
                })
                .ToListAsync();

            var sales = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Buyer)
                .Where(t => t.SellerId == myUserId)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionListItemDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Price = t.Price,
                    ShippingAddress = t.ShippingAddress,
                    Status = t.Status,
                    ItemName = t.Item != null ? t.Item.Name : "Item não disponível",
                    OtherPartyName = t.Buyer != null ? t.Buyer.Name : "Comprador desconhecido",
                    TransactionType = "Venda"
                })
                .ToListAsync();

            var result = new TransactionListResponseDto
            {
                Purchases = purchases,
                Sales = sales
            };

            return Ok(result);
        }

        /// <summary>
        /// GET: api/TransactionsApi/5
        /// Obtém uma transação específica
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailResponseDto>> GetTransaction(int id)
        {
            var myUserId = await GetCurrentMyUserId();
            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var transaction = await _context.Transactions
                .Include(t => t.Item)
                .Include(t => t.Seller)
                .Include(t => t.Buyer)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound(new { message = $"Transação com ID {id} não encontrada." });
            }

            // Verificar se o utilizador é parte da transação
            if (transaction.BuyerId != myUserId && transaction.SellerId != myUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var result = new TransactionDetailResponseDto
            {
                Id = transaction.Id,
                Date = transaction.Date,
                Price = transaction.Price,
                ShippingAddress = transaction.ShippingAddress,
                Status = transaction.Status,
                Item = transaction.Item != null ? new TransactionPartyDto
                {
                    Id = transaction.Item.Id,
                    Name = transaction.Item.Name,
                    Description = transaction.Item.Description
                } : null,
                Seller = transaction.Seller != null ? new TransactionPartyDto
                {
                    Id = transaction.Seller.Id,
                    Name = transaction.Seller.Name
                } : null,
                Buyer = transaction.Buyer != null ? new TransactionPartyDto
                {
                    Id = transaction.Buyer.Id,
                    Name = transaction.Buyer.Name
                } : null
            };

            return Ok(result);
        }

        /// <summary>
        /// POST: api/TransactionsApi
        /// Cria uma nova transação (compra)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var buyerId = await GetCurrentMyUserId();
            if (buyerId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            // Verificar se o item existe e está disponível
            var item = await _context.Items
                .Include(i => i.UserItems)
                .FirstOrDefaultAsync(i => i.Id == createTransactionDto.ItemId);

            if (item == null)
            {
                return NotFound(new { message = "Item não encontrado." });
            }

            if (item.Status != "Disponível")
            {
                return BadRequest(new { message = "Este item não está disponível para compra." });
            }

            // Obter o vendedor (dono do item)
            var sellerId = item.UserItems.FirstOrDefault()?.UserId;
            if (sellerId == null)
            {
                return BadRequest(new { message = "Item sem vendedor associado." });
            }

            if (sellerId == buyerId)
            {
                return BadRequest(new { message = "Não pode comprar o seu próprio item." });
            }

            // Criar a transação
            var transaction = new Transaction
            {
                SellerId = sellerId.Value,
                BuyerId = buyerId,
                ItemId = item.Id,
                Price = item.Price,
                ShippingAddress = createTransactionDto.ShippingAddress,
                Date = DateTime.Now,
                Status = "Pendente"
            };

            _context.Transactions.Add(transaction);

            // Atualizar o status do item
            item.Status = "Vendido";

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }

        /// <summary>
        /// PUT: api/TransactionsApi/{id}/Status
        /// Atualiza o status de uma transação
        /// </summary>
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> UpdateTransactionStatus(int id, [FromBody] UpdateTransactionStatusDto statusDto)
        {
            var myUserId = await GetCurrentMyUserId();
            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var transaction = await _context.Transactions
                .Include(t => t.Item)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound(new { message = $"Transação com ID {id} não encontrada." });
            }

            // Verificar se o utilizador é o vendedor
            if (transaction.SellerId != myUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Status válidos: "Pendente", "Confirmada", "Enviada", "Entregue", "Cancelada"
            var validStatuses = new[] { "Pendente", "Confirmada", "Enviada", "Entregue", "Cancelada" };
            if (!validStatuses.Contains(statusDto.Status))
            {
                return BadRequest(new { message = "Status inválido. Use: Pendente, Confirmada, Enviada, Entregue ou Cancelada." });
            }

            transaction.Status = statusDto.Status;

            // Se a transação for cancelada, devolver o item ao estado disponível
            if (statusDto.Status == "Cancelada")
            {
                if (transaction.Item != null)
                {
                    transaction.Item.Status = "Disponível";
                }
            }

            // Se a transação for entregue, marcar como concluída
            if (statusDto.Status == "Entregue")
            {
                transaction.Status = "Concluída";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Status da transação atualizado com sucesso.", status = transaction.Status });
        }

        /// <summary>
        /// GET: api/TransactionsApi/Stats
        /// Obtém estatísticas de transações do utilizador
        /// </summary>
        [HttpGet("Stats")]
        public async Task<ActionResult<TransactionStatsDto>> GetTransactionStats()
        {
            var myUserId = await GetCurrentMyUserId();
            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var stats = new TransactionStatsDto
            {
                TotalSales = await _context.Transactions
                    .Where(t => t.SellerId == myUserId && t.Status == "Concluída")
                    .SumAsync(t => t.Price),
                TotalPurchases = await _context.Transactions
                    .Where(t => t.BuyerId == myUserId && t.Status == "Concluída")
                    .SumAsync(t => t.Price),
                SalesCount = await _context.Transactions
                    .CountAsync(t => t.SellerId == myUserId),
                PurchasesCount = await _context.Transactions
                    .CountAsync(t => t.BuyerId == myUserId)
            };

            return Ok(stats);
        }

        /// <summary>
        /// GET: api/TransactionsApi/Item/{itemId}
        /// Obtém o histórico de transações de um item específico
        /// </summary>
        [HttpGet("Item/{itemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TransactionListItemDto>>> GetItemTransactions(int itemId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Where(t => t.ItemId == itemId)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionListItemDto
                {
                    Id = t.Id,
                    Date = t.Date,
                    Price = t.Price,
                    ShippingAddress = t.ShippingAddress,
                    Status = t.Status,
                    ItemName = t.Item != null ? t.Item.Name : "Item não disponível",
                    OtherPartyName = t.Buyer != null ? t.Buyer.Name : "Comprador desconhecido",
                    TransactionType = "Histórico"
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // Métodos auxiliares
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
}