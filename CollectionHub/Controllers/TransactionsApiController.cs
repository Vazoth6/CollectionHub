using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TransactionsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionReadDTO>>> GetTransactions()
    {
        return await _context.Transactions
            .Include(t => t.Seller)
            .Include(t => t.Buyer)
            .Include(t => t.Item)
            .OrderByDescending(t => t.Date)
            .Select(t => ToReadDTO(t))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionReadDTO>> GetTransaction(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Seller)
            .Include(t => t.Buyer)
            .Include(t => t.Item)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null) return NotFound();
        return ToReadDTO(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionReadDTO>> CreateTransaction(TransactionCreateDTO dto)
    {
        var validation = await ValidateForeignKeys(dto.SellerId, dto.BuyerId, dto.ItemId);
        if (validation != null) return validation;

        var transaction = new Transaction
        {
            SellerId = dto.SellerId,
            BuyerId = dto.BuyerId,
            ItemId = dto.ItemId,
            Price = dto.Price,
            ShippingAddress = dto.ShippingAddress,
            Status = dto.Status,
            Date = DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _context.Entry(transaction).Reference(t => t.Seller).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.Buyer).LoadAsync();
        await _context.Entry(transaction).Reference(t => t.Item).LoadAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, ToReadDTO(transaction));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTransaction(int id, TransactionUpdateDTO dto)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        var validation = await ValidateForeignKeys(dto.SellerId, dto.BuyerId, dto.ItemId);
        if (validation != null) return validation;

        transaction.SellerId = dto.SellerId;
        transaction.BuyerId = dto.BuyerId;
        transaction.ItemId = dto.ItemId;
        transaction.Price = dto.Price;
        transaction.ShippingAddress = dto.ShippingAddress;
        transaction.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<BadRequestObjectResult?> ValidateForeignKeys(int sellerId, int buyerId, int itemId)
    {
        if (sellerId == buyerId) return BadRequest("O vendedor e o comprador têm de ser utilizadores diferentes.");
        if (!await _context.MyUsers.AnyAsync(u => u.Id == sellerId)) return BadRequest("O vendedor indicado não existe.");
        if (!await _context.MyUsers.AnyAsync(u => u.Id == buyerId)) return BadRequest("O comprador indicado não existe.");
        if (!await _context.Items.AnyAsync(i => i.Id == itemId)) return BadRequest("O item indicado não existe.");
        return null;
    }

    private static TransactionReadDTO ToReadDTO(Transaction transaction) => new()
    {
        Id = transaction.Id,
        SellerId = transaction.SellerId,
        SellerName = transaction.Seller?.Name,
        BuyerId = transaction.BuyerId,
        BuyerName = transaction.Buyer?.Name,
        ItemId = transaction.ItemId,
        ItemName = transaction.Item?.Name,
        Date = transaction.Date,
        Price = transaction.Price,
        ShippingAddress = transaction.ShippingAddress,
        Status = transaction.Status
    };
}
