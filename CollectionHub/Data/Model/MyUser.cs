using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.ServerSentEvents;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [RegularExpression("[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}", ErrorMessage = " O {0} não é um email válido. Insira um email válido.")]
        public string Email { get; set; } = string.Empty;

        // Nunca guardar passwords normais
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = "User";

        public DateTime RegisterDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string UserID { get; set; } = string.Empty;

        // =========================
        // RELACIONAMENTOS
        // =========================

        // Itens do utilizador
        public ICollection<UserItem> UserItems { get; set; }
            = new List<UserItem>();

        // Transações como vendedor
        public ICollection<Transaction> Sales { get; set; }
            = new List<Transaction>();

        // Transações como comprador
        public ICollection<Transaction> Purchases { get; set; }
            = new List<Transaction>();
    }
}