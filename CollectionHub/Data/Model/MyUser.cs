using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Nunca guardar passwords normais
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = "User";

        public DateTime RegisterDate { get; set; } = DateTime.Now;

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