using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// GET: api/UsersApi/Profile
        /// Obtém o perfil do utilizador autenticado
        /// </summary>
        [HttpGet("Profile")]
        public async Task<ActionResult<object>> GetMyProfile()
        {
            var myUserId = await GetCurrentMyUserId();
            if (myUserId == 0)
            {
                return Unauthorized(new { message = "Utilizador não autenticado." });
            }

            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.Id == myUserId);

            if (myUser == null)
            {
                return NotFound(new { message = "Perfil de utilizador não encontrado." });
            }

            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);

            var stats = new
            {
                ItemsForSale = await _context.UserItems.CountAsync(ui => ui.UserId == myUserId),
                CompletedSales = await _context.Transactions.CountAsync(t => t.SellerId == myUserId && t.Status == "Entregue"),
                CompletedPurchases = await _context.Transactions.CountAsync(t => t.BuyerId == myUserId && t.Status == "Entregue"),
                TotalRevenue = await _context.Transactions
                    .Where(t => t.SellerId == myUserId && t.Status == "Entregue")
                    .SumAsync(t => t.Price)
            };

            var result = new
            {
                myUser.Id,
                myUser.Name,
                myUser.CellPhone,
                myUser.Role,
                myUser.RegisterDate,
                Email = identityUser?.Email ?? string.Empty,
                Statistics = stats
            };

            return Ok(result);
        }

        /// <summary>
        /// GET: api/UsersApi/Profile/{id}
        /// Obtém o perfil de um utilizador específico (apenas Admin)
        /// </summary>
        [HttpGet("Profile/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUserProfile(int id)
        {
            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (myUser == null)
            {
                return NotFound(new { message = $"Utilizador com ID {id} não encontrado." });
            }

            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);

            var result = new
            {
                myUser.Id,
                myUser.Name,
                myUser.CellPhone,
                myUser.Role,
                myUser.RegisterDate,
                Email = identityUser?.Email ?? string.Empty
            };

            return Ok(result);
        }

        /// <summary>
        /// GET: api/UsersApi/All
        /// Obtém todos os utilizadores (apenas Admin)
        /// </summary>
        [HttpGet("All")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _context.MyUsers
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.CellPhone,
                    u.Role,
                    u.RegisterDate,
                    u.UserID
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// PUT: api/UsersApi/Profile
        /// Atualiza o perfil do utilizador autenticado
        /// </summary>
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto updateProfileDto)
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

            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.Id == myUserId);

            if (myUser == null)
            {
                return NotFound(new { message = "Perfil de utilizador não encontrado." });
            }

            myUser.Name = updateProfileDto.Name;
            myUser.CellPhone = updateProfileDto.CellPhone;

            _context.Entry(myUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MyUserExists(myUserId))
                {
                    return NotFound(new { message = "Perfil de utilizador não encontrado." });
                }
                throw;
            }

            return Ok(new { message = "Perfil atualizado com sucesso.", myUser });
        }

        /// <summary>
        /// PUT: api/UsersApi/Profile/Role/{userId}
        /// Atualiza o role de um utilizador (apenas Admin)
        /// </summary>
        [HttpPut("Profile/Role/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateRoleDto updateRoleDto)
        {
            var myUser = await _context.MyUsers
                .FirstOrDefaultAsync(m => m.Id == userId);

            if (myUser == null)
            {
                return NotFound(new { message = $"Utilizador com ID {userId} não encontrado." });
            }

            // Validar role
            var validRoles = new[] { "Utilizador", "Vendedor", "Admin" };
            if (!validRoles.Contains(updateRoleDto.Role))
            {
                return BadRequest(new { message = "Role inválido. Use: Utilizador, Vendedor ou Admin." });
            }

            myUser.Role = updateRoleDto.Role;

            // Atualizar também o role no Identity (opcional)
            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);
            if (identityUser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(identityUser);
                await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                await _userManager.AddToRoleAsync(identityUser, updateRoleDto.Role);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Role do utilizador {myUser.Name} atualizado para {updateRoleDto.Role}." });
        }

        /// <summary>
        /// DELETE: api/UsersApi/Profile/{userId}
        /// Elimina um utilizador (apenas Admin)
        /// </summary>
        [HttpDelete("Profile/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var myUser = await _context.MyUsers
                .Include(u => u.UserItems)
                .Include(u => u.Sales)
                .Include(u => u.Purchases)
                .FirstOrDefaultAsync(m => m.Id == userId);

            if (myUser == null)
            {
                return NotFound(new { message = $"Utilizador com ID {userId} não encontrado." });
            }

            // Verificar se o utilizador tem transações ativas
            if (myUser.Sales.Any(s => s.Status != "Entregue" && s.Status != "Cancelada") ||
                myUser.Purchases.Any(p => p.Status != "Entregue" && p.Status != "Cancelada"))
            {
                return BadRequest(new { message = "Não é possível eliminar um utilizador com transações pendentes." });
            }

            // Eliminar associações UserItem
            _context.UserItems.RemoveRange(myUser.UserItems);

            // Eliminar transações (já verificadas)
            _context.Transactions.RemoveRange(myUser.Sales);
            _context.Transactions.RemoveRange(myUser.Purchases);

            // Eliminar MyUser
            _context.MyUsers.Remove(myUser);

            // Eliminar IdentityUser
            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Utilizador {myUser.Name} eliminado com sucesso." });
        }

        /// <summary>
        /// GET: api/UsersApi/Sellers
        /// Obtém todos os vendedores (para listar na criação de itens)
        /// </summary>
        [HttpGet("Sellers")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetSellers()
        {
            var sellers = await _context.MyUsers
                .Where(u => u.Role == "Vendedor" || u.Role == "Admin")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    ItemsForSale = _context.UserItems.Count(ui => ui.UserId == u.Id)
                })
                .ToListAsync();

            return Ok(sellers);
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

        private bool MyUserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}