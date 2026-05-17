using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class Transaction
    {
        [Key]
        public long Id { get; set; }

        // =========================
        // VENDEDOR
        // =========================

        public long SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public MyUser? Seller { get; set; }

        // =========================
        // COMPRADOR
        // =========================

        public long BuyerId { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public MyUser? Buyer { get; set; }

        // =========================
        // ITEM
        // =========================

        public long ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        // =========================

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Concluída";
    }
}