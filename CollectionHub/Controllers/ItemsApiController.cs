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
    public class ItemsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItems([FromQuery] ItemListQueryDto query)
        {
            var dbQuery = _context.Items
                .Include(i => i.Category)
                .Include(i => i.UserItems)
                .ThenInclude(ui => ui.User)
                .Where(i => i.Status == "Disponível")
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                dbQuery = dbQuery.Where(i => i.Name.Contains(query.SearchTerm) ||
                                             (i.Description != null && i.Description.Contains(query.SearchTerm)));
            }

            if (query.CategoryId.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.CategoryId == query.CategoryId.Value);
            }

            if (query.SelectedCategories != null && query.SelectedCategories.Any())
            {
                dbQuery = dbQuery.Where(i => query.SelectedCategories.Contains(i.Category.Name));
            }

            if (query.MinPrice.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.Price >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.Price <= query.MaxPrice.Value);
            }

            dbQuery = query.SortBy switch
            {
                "price_asc" => dbQuery.OrderBy(i => i.Price),
                "price_desc" => dbQuery.OrderByDescending(i => i.Price),
                "name_desc" => dbQuery.OrderByDescending(i => i.Name),
                _ => dbQuery.OrderBy(i => i.Name)
            };

            var totalItems = await dbQuery.CountAsync();

            var items = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var result = items.Select(i => new ItemResponseDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                Status = i.Status,
                ImageUrl = i.ImageUrl,
                CategoryId = i.CategoryId,
                CategoryName = i.Category?.Name ?? "Sem Categoria",
                SellerName = i.UserItems.FirstOrDefault()?.User?.Name ?? "Vendedor Desconhecido",
                SellerId = i.UserItems.FirstOrDefault()?.UserId
            });

            Response.Headers.Append("X-Total-Count", totalItems.ToString());
            Response.Headers.Append("X-Total-Pages", Math.Ceiling(totalItems / (double)query.PageSize).ToString());

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ItemResponseDto>> GetItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.UserItems)
                .ThenInclude(ui => ui.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado." });
            }

            var result = new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Status = item.Status,
                ImageUrl = item.ImageUrl,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name ?? "Sem Categoria",
                SellerName = item.UserItems.FirstOrDefault()?.User?.Name ?? "Vendedor Desconhecido",
                SellerId = item.UserItems.FirstOrDefault()?.UserId
            };

            return Ok(result);
        }

        [HttpGet("User/MyItems")]
        public async Task<ActionResult<IEnumerable<MyItemResponseDto>>> GetMyItems()
        {
            var myUserId = await GetCurrentMyUserId();

            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var userItems = await _context.UserItems
                .Include(ui => ui.Item)
                .ThenInclude(i => i.Category)
                .Where(ui => ui.UserId == myUserId && ui.Item != null)
                .Select(ui => ui.Item!)
                .ToListAsync();

            var result = userItems.Select(i => new MyItemResponseDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                Status = i.Status,
                CategoryId = i.CategoryId,
                CategoryName = i.Category?.Name ?? "Sem Categoria"
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ItemResponseDto>> PostItem([FromBody] CreateItemDto createItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var myUserId = await GetCurrentMyUserId();

            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description ?? string.Empty,
                Price = createItemDto.Price,
                CategoryId = createItemDto.CategoryId,
                Status = "Disponível",
                ImageUrl = createItemDto.ImageUrl,
                SubmittedAt = DateTime.Now
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var userItem = new UserItem
            {
                UserId = myUserId,
                ItemId = item.Id
            };

            _context.UserItems.Add(userItem);
            await _context.SaveChangesAsync();

            var seller = await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == myUserId);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == item.CategoryId);

            var response = new ItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Status = item.Status,
                ImageUrl = item.ImageUrl,
                CategoryId = item.CategoryId,
                CategoryName = category?.Name ?? "Sem Categoria",
                SellerName = seller?.Name ?? "Vendedor",
                SellerId = myUserId
            };

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, response);
        }

        [HttpPost("Buy/{id}")]
        public async Task<IActionResult> BuyItem(int id, [FromBody] BuyItemRequest request)
        {
            var buyerId = await GetCurrentMyUserId();

            if (buyerId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var item = await _context.Items
                .Include(i => i.UserItems)
                .ThenInclude(ui => ui.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound(new { message = "Item não encontrado." });
            }

            if (item.Status != "Disponível")
            {
                return BadRequest(new { message = "Este item não está disponível." });
            }

            var sellerId = item.UserItems.FirstOrDefault()?.UserId;

            if (sellerId == null)
            {
                return BadRequest(new { message = "Item sem vendedor." });
            }

            if (sellerId == buyerId)
            {
                return BadRequest(new { message = "Não pode comprar o seu próprio item." });
            }

            var buyer = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == buyerId);
            var seller = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == sellerId.Value);

            if (buyer == null || seller == null)
            {
                return BadRequest(new { message = "Utilizador não encontrado." });
            }

            if (buyer.WalletBalance < item.Price)
            {
                return BadRequest(new { message = "Saldo insuficiente. Recarregue a sua carteira." });
            }

            buyer.WalletBalance -= item.Price;
            seller.WalletBalance += item.Price;

            item.Status = "Vendido";

            var transaction = new Transaction
            {
                SellerId = sellerId.Value,
                BuyerId = buyerId,
                ItemId = item.Id,
                Price = item.Price,
                ShippingAddress = request.ShippingAddress,
                Date = DateTime.Now,
                Status = "Pago",
                PaymentMethod = "Carteira Virtual",
                IsPaid = true,
                PaymentDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Este item já não está disponível. Atualize a página e tente novamente."
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Compra realizada com sucesso! Saldo restante: {buyer.WalletBalance:C}",
                newBalance = buyer.WalletBalance
            });
        }

        public class BuyItemRequest
        {
            public string ShippingAddress { get; set; } = string.Empty;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, [FromBody] UpdateItemDto updateItemDto)
        {
            if (id != updateItemDto.Id)
            {
                return BadRequest(new { message = "ID do item não corresponde." });
            }

            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado." });
            }

            if (!await IsItemOwner(id) && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            item.Name = updateItemDto.Name;
            item.Description = updateItemDto.Description ?? string.Empty;
            item.Price = updateItemDto.Price;
            item.CategoryId = updateItemDto.CategoryId;
            item.Status = updateItemDto.Status ?? item.Status;

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound(new { message = $"Item com ID {id} não encontrado." });
                }

                throw;
            }

            return Ok(new { message = "Item atualizado com sucesso.", item });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new { message = $"Item com ID {id} não encontrado." });
            }

            if (!await IsItemOwner(id) && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var hasTransactions = await _context.Transactions.AnyAsync(t => t.ItemId == id);

            if (hasTransactions)
            {
                return BadRequest(new { message = "Não é possível eliminar um item que tem transações associadas." });
            }

            var userItems = _context.UserItems.Where(ui => ui.ItemId == id);
            _context.UserItems.RemoveRange(userItems);

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item eliminado com sucesso." });
        }

        [HttpGet("Categories/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ItemResponseDto>>> GetItemsByCategory(int categoryId)
        {
            var items = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId && i.Status == "Disponível")
                .Select(i => new ItemResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    Status = i.Status,
                    CategoryId = i.CategoryId,
                    CategoryName = i.Category != null ? i.Category.Name : "Sem Categoria"
                })
                .ToListAsync();

            return Ok(items);
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
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

        private async Task<bool> IsItemOwner(int itemId)
        {
            var userId = await GetCurrentMyUserId();

            return await _context.UserItems
                .AnyAsync(ui => ui.UserId == userId && ui.ItemId == itemId);
        }
    }
}