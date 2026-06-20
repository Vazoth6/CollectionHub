using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class ItemLike
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public MyUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}