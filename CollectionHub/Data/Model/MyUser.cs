using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome do utilizador
        /// </summary>
        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email do utilizador
        /// </summary>
        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = "Utilizador";

        /// <summary>
        /// Número de telemóvel do utilizador
        /// </summary>
        [Display(Name = "Telemóvel")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Formato de telemóvel inválido")]
        public string? CellPhone { get; set; }

        /// <summary>
        /// Data de registo do utilizador
        /// </summary>
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        /// <summary>
        /// ID do utilizador no sistema de autenticação (Identity)
        /// </summary>
        [StringLength(450)]
        public string UserID { get; set; } = "";

        /// <summary>
        /// Saldo da carteira virtual do utilizador
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo")]
        public decimal WalletBalance { get; set; } = 5000.00m;

        /// <summary>
        /// Uma coleção de itens que o utilizador possui (relação 1:N com UserItem)
        /// </summary>
        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        /// <summary>
        /// Vendas realizadas pelo utilizador (relação 1:N com Transaction)
        /// </summary>
        public ICollection<Transaction> Sales { get; set; } = new List<Transaction>();

        /// <summary>
        /// Compras realizadas pelo utilizador (relação 1:N com Transaction)
        /// </summary>
        public ICollection<Transaction> Purchases { get; set; } = new List<Transaction>();

        /// <summary>
        /// Gostos do utilizador (relação 1:N com ItemLike)
        /// </summary>
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    }
}