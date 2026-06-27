using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    // <summary>
    // Representa uma compra ou venda efectuada na plataforma.
    // </summary>
    public class Transaction
    {
        [Key]
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }

        // <summary>
        // FK para o vendedor (MyUser)
        // </summary>

        public int SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        // <summary>
        // Obtém ou define vendedor.
        // </summary>
        public MyUser? Seller { get; set; }

        // <summary>
        // FK para o comprador (MyUser)
        // </summary>
        public int BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        // <summary>
        // Obtém ou define comprador.
        // </summary>
        public MyUser? Buyer { get; set; }

        // <summary>
        // FK para o item
        // </summary>
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        // <summary>
        // Obtém ou define artigo.
        // </summary>
        public Item? Item { get; set; }

        // <summary>
        // Data em que a transação ocorreu
        // </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }

        // <summary>
        // Endereço de entrega do item comprado
        // </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Endereço de Entrega")]
        public string ShippingAddress { get; set; } = string.Empty;

        // <summary>
        // Estado da transação
        // </summary>
        public string Status { get; set; } = "Pendente";

        // <summary>
        // Método de pagamento utilizado na transação
        // </summary>
        public string PaymentMethod { get; set; } = "Carteira Virtual";

        // <summary>
        // Verifica se a transação foi paga ou não
        // </summary>
        public bool IsPaid { get; set; } = false;

        // <summary>
        // Data em que o pagamento foi realizado
        // </summary>
        public DateTime? PaymentDate { get; set; }
    }
}
