using CollectionHub.Data;
using CollectionHub.Data.Model;
using CollectionHub.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Controllers;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetUsers()
    {
        return await _context.MyUsers
            .OrderBy(u => u.Name)
            .Select(u => ToReadDTO(u))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserReadDTO>> GetUser(int id)
    {
        var user = await _context.MyUsers.FindAsync(id);
        if (user == null) return NotFound();
        return ToReadDTO(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserReadDTO>> CreateUser(UserCreateDTO dto)
    {
        var user = new MyUser
        {
            Name = dto.Name,
            Role = dto.Role,
            CellPhone = dto.CellPhone,
            UserID = dto.UserID,
            RegisterDate = DateTime.Now
        };

        _context.MyUsers.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToReadDTO(user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDTO dto)
    {
        var user = await _context.MyUsers.FindAsync(id);
        if (user == null) return NotFound();

        user.Name = dto.Name;
        user.Role = dto.Role;
        user.CellPhone = dto.CellPhone;
        user.UserID = dto.UserID;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.MyUsers
            .Include(u => u.UserItems)
            .Include(u => u.Sales)
            .Include(u => u.Purchases)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();
        if (user.Sales.Any() || user.Purchases.Any())
            return BadRequest("Não é possível eliminar um utilizador com transações associadas.");

        _context.UserItems.RemoveRange(user.UserItems);
        _context.MyUsers.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static UserReadDTO ToReadDTO(MyUser user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Role = user.Role,
        CellPhone = user.CellPhone,
        RegisterDate = user.RegisterDate,
        UserID = user.UserID
    };
}
