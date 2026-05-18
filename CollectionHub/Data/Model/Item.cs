using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.ServerSentEvents;

namespace CollectionHub.Data.Model
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Disponível";

        // =========================
        // FOREIGN KEY
        // =========================

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // =========================
        // RELACIONAMENTOS
        // =========================

        public ICollection<UserItem> UserItems { get; set; }
            = new List<UserItem>();

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}