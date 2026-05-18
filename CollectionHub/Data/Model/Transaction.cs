using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        // =========================
        // VENDEDOR
        // =========================

        /// <summary>
        /// FK para o vendedor (MyUser)
        /// </summary>

        public int SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public MyUser? Seller { get; set; }

        // =========================
        // COMPRADOR
        // =========================


        /// <summary>
        /// FK para o comprador (MyUser)
        /// </summary>
        public int BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public MyUser? Buyer { get; set; }

        // =========================
        // ITEM
        // =========================

        /// <summary>
        /// FK para o item (Item)
        /// </summary>

        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        // =========================

        /// <summary>
        /// Data em que a transação ocorreu
        /// </summary>

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Concluída";
    }
}