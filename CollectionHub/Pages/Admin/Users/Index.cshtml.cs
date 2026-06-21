using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;

namespace CollectionHub.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<UserAdminDto> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.MyUsers
                .Select(u => new UserAdminDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = _context.Users.FirstOrDefault(i => i.Id == u.UserID).Email ?? "N/A",
                    CellPhone = u.CellPhone,
                    Role = u.Role,
                    RegisterDate = u.RegisterDate
                })
                .OrderByDescending(u => u.RegisterDate)
                .ToListAsync();
        }
    }

    public class UserAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CellPhone { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime RegisterDate { get; set; }
    }
}