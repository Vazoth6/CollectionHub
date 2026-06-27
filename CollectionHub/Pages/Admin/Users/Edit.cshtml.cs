using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CollectionHub.Data;
using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    // <summary>
    // Representa o modelo de dados utilizado para o edit model.
    // </summary>
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
        // <summary>
        // Obtém ou define edit user.
        // </summary>
        public EditUserDto EditUser { get; set; } = new();

        // <summary>
        // Obtém ou define lista de seleção de perfis.
        // </summary>
        public List<SelectListItem> RolesSelectList { get; set; } = new();

        // <summary>
        // Carrega os dados necessários para apresentar a página ao utilizador.
        // </summary>
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

            EditUser = new EditUserDto
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

        // <summary>
        // Processa o formulário submetido pelo utilizador.
        // </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoles();
                return Page();
            }

            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.Id == EditUser.Id);
            if (myUser == null)
            {
                return NotFound();
            }

            myUser.Name = EditUser.Name;
            myUser.CellPhone = EditUser.CellPhone;
            myUser.Role = EditUser.Role;

            var identityUser = await _userManager.FindByIdAsync(myUser.UserID);
            if (identityUser != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(identityUser);
                await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                await _userManager.AddToRoleAsync(identityUser, EditUser.Role);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Utilizador '{EditUser.Name}' actualizado com sucesso!";
            return RedirectToPage("/Admin/Users/Index");
        }

        private async Task LoadRoles()
        {
            var roles = new[] { "Utilizador", "Vendedor", "Admin" };

            RolesSelectList = roles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == EditUser.Role
            }).ToList();

            await Task.CompletedTask;
        }
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para edit user dto.
    // </summary>
    public class EditUserDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;

        // <summary>
        // Obtém ou define email.
        // </summary>
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telemóvel")]
        // <summary>
        // Obtém ou define telemóvel.
        // </summary>
        public string? CellPhone { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [Display(Name = "Cargo")]
        // <summary>
        // Obtém ou define perfil.
        // </summary>
        public string Role { get; set; } = string.Empty;
    }
}
