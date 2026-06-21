using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using CollectionHub.Data.Model;
using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public EditUserDto User { get; set; } = new();

        public List<SelectListItem> RolesSelectList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (myUser == null)
            {
                return NotFound();
            }

            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);
            if (identityUser == null)
            {
                return NotFound();
            }

            User = new EditUserDto
            {
                Id = myUser.Id,
                Name = myUser.Name,
                Email = identityUser.Email ?? string.Empty,
                CellPhone = myUser.CellPhone,
                Role = myUser.Role
            };

            await LoadRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoles();
                return Page();
            }

            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == User.Id);
            if (myUser == null)
            {
                return NotFound();
            }

            myUser.Name = User.Name;
            myUser.CellPhone = User.CellPhone;
            myUser.Role = User.Role;

            // Atualizar o role no Identity também
            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);
            if (identityUser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(identityUser);
                await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                await _userManager.AddToRoleAsync(identityUser, User.Role);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Utilizador '{User.Name}' atualizado com sucesso!";
            return RedirectToPage("/Admin/Users/Index");
        }

        private async Task LoadRoles()
        {
            var roles = new[] { "Utilizador", "Vendedor", "Admin" };
            RolesSelectList = roles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == User.Role
            }).ToList();
        }
    }

    public class EditUserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telemóvel")]
        public string? CellPhone { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = string.Empty;
    }
}