using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [Display(Name = "Estado")]
        public string Status { get; set; } = "Disponível";

        [Display(Name = "Imagem")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Data de submissão")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
