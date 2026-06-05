using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ItemsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemReadDTO>>> GetItems()
    {
        return await _context.Items
            .Include(i => i.Category)
            .OrderByDescending(i => i.SubmittedAt)
            .Select(i => ToReadDTO(i))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemReadDTO>> GetItem(int id)
    {
        var item = await _context.Items.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();

        return ToReadDTO(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemReadDTO>> CreateItem(ItemCreateDTO dto)
    {
        if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest("A categoria indicada não existe.");

        var item = new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Status = dto.Status,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            SubmittedAt = DateTime.Now
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        await _context.Entry(item).Reference(i => i.Category).LoadAsync();
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, ToReadDTO(item));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, ItemUpdateDTO dto)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest("A categoria indicada não existe.");

        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.Status = dto.Status;
        item.ImageUrl = dto.ImageUrl;
        item.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.Items
            .Include(i => i.Transactions)
            .Include(i => i.UserItems)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return NotFound();
        if (item.Transactions.Any()) return BadRequest("Não é possível eliminar um item com transações associadas.");

        _context.UserItems.RemoveRange(item.UserItems);
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static ItemReadDTO ToReadDTO(Item item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        Status = item.Status,
        ImageUrl = item.ImageUrl,
        SubmittedAt = item.SubmittedAt,
        CategoryId = item.CategoryId,
        CategoryName = item.Category?.Name
    };
}
