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
    public class CategoriesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/CategoriesApi
        /// Obtém todas as categorias
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ItemCount = _context.Items.Count(i => i.CategoryId == c.Id && i.Status == "Disponível")
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// GET: api/CategoriesApi/5
        /// Obtém uma categoria específica pelo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDetailResponseDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"Categoria com ID {id} não encontrada." });
            }

            var result = new CategoryDetailResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Items = category.Items.Select(i => new CategoryItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Status = i.Status
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// POST: api/CategoriesApi
        /// Cria uma nova categoria (apenas Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Category>> PostCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar se já existe categoria com o mesmo nome
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == createCategoryDto.Name);

            if (existingCategory != null)
            {
                return BadRequest(new { message = "Já existe uma categoria com este nome." });
            }

            var category = new Category
            {
                Name = createCategoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        /// <summary>
        /// PUT: api/CategoriesApi/5
        /// Atualiza uma categoria (apenas Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            if (id != updateCategoryDto.Id)
            {
                return BadRequest(new { message = "ID da categoria não corresponde." });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = $"Categoria com ID {id} não encontrada." });
            }

            // Verificar se o novo nome já existe (exceto a própria categoria)
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == updateCategoryDto.Name && c.Id != id);

            if (existingCategory != null)
            {
                return BadRequest(new { message = "Já existe outra categoria com este nome." });
            }

            category.Name = updateCategoryDto.Name;
            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound(new { message = $"Categoria com ID {id} não encontrada." });
                }
                throw;
            }

            return Ok(new { message = "Categoria atualizada com sucesso.", category });
        }

        /// <summary>
        /// DELETE: api/CategoriesApi/5
        /// Elimina uma categoria (apenas Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"Categoria com ID {id} não encontrada." });
            }

            // Verificar se existem itens nesta categoria
            if (category.Items.Any())
            {
                return BadRequest(new { message = "Não é possível eliminar uma categoria que tem itens associados." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Categoria eliminada com sucesso." });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}