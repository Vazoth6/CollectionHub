using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    public class UserItem
    {
        /// <summary>
        /// Identificador único da relação entre utilizador e item.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Utilizador associado a este item.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public MyUser? User { get; set; }

        /// <summary>
        /// Identificador único do item associado ao utilizador.
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Item associado a este utilizador.
        /// </summary>
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }
    }
}