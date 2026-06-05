using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryReadDTO>>> GetCategories()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryReadDTO { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryReadDTO>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return new CategoryReadDTO { Id = category.Id, Name = category.Name };
    }

    [HttpPost]
    public async Task<ActionResult<CategoryReadDTO>> CreateCategory(CategoryCreateDTO dto)
    {
        var category = new Category { Name = dto.Name };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var readDto = new CategoryReadDTO { Id = category.Id, Name = category.Name };
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, readDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDTO dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        category.Name = dto.Name;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        if (category.Items.Any()) return BadRequest("Não é possível eliminar uma categoria com itens associados.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
