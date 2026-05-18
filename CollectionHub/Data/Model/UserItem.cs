using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class UserItem
    {
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public MyUser? User { get; set; }

        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }
    }
}