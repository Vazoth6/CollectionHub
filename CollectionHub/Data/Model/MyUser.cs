using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.ServerSentEvents;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        [Key] // PK
        public int Id { get; set; }

        /// <summary>
        /// Nome do utilizador
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = "Utilizador";

        /// <summary>
        /// Número de telemóvel do utilizador
        /// </summary>
        [Display(Name = "Telemóvel")]
        [StringLength(20)]
        [RegularExpression(@"^(\+[0-9]{1,3})?[0-9]{9,12}$",
            ErrorMessage = "O número de telemóvel deve conter apenas dígitos e pode começar opcionalmente com um + e o indicativo do país.")]
        [Phone(ErrorMessage = "Formato de telemóvel inválido")]
        public string? CellPhone { get; set; }

        /// <summary>
        /// Data de registro
        /// </summary>
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Atributo para funcionar como FK entre a tabela dos MyUser
        /// e a tabela da Autenticação
        /// </summary>
        [StringLength(50)]
        public string UserID { get; set; } = "";

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