using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// FK para o vendedor (MyUser)
        /// </summary>

        public int SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public MyUser? Seller { get; set; }

        /// <summary>
        /// FK para o comprador (MyUser)
        /// </summary>
        public int BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public MyUser? Buyer { get; set; }

        /// <summary>
        /// FK para o item (Item)
        /// </summary>
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        /// <summary>
        /// Data em que a transação ocorreu
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Endereço de entrega do item comprado
        /// </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Endereço de Entrega")]
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>
        /// Estado da transação
        /// </summary>
        public string Status { get; set; } = "Pendente";

        /// <summary>
        /// Método de pagamento utilizado na transação
        /// </summary>
        public string PaymentMethod { get; set; } = "Carteira Virtual";

        /// <summary>
        /// Verifica se a transação foi paga ou não
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Data em que o pagamento foi realizado
        /// </summary>
        public DateTime? PaymentDate { get; set; }
    }
}