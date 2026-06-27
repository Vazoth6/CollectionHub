using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model
{
    public class Category
    {
        /// <summary>
        /// PK
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da categoria
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Items associados a esta categoria (relação 1:N com Item)
        /// </summary>
        public ICollection<Item> Items { get; set; }
            = new List<Item>();
    }
}