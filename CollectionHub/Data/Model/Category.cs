using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Relação 1:N
        public ICollection<Item> Items { get; set; }
            = new List<Item>();
    }
}