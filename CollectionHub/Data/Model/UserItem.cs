using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    // <summary>
    // Representa a relação entre um utilizador e um artigo publicado por esse utilizador.
    // </summary>
    public class UserItem
    {
        // <summary>
        // Id único da relação entre utilizador e artigo.
        // </summary>
        public int UserId { get; set; }

        // <summary>
        // Utilizador associado a este artigo.
        // </summary>
        [ForeignKey(nameof(UserId))]
        public MyUser? User { get; set; }

        // <summary>
        // Id único do item associado ao utilizador.
        // </summary>
        public int ItemId { get; set; }

        // <summary>
        // Item associado a este utilizador.
        // </summary>
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }
    }
}
