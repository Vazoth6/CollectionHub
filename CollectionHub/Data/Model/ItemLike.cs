using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectionHub.Data.Model
{
    // <summary>
    // Representa um gosto atribuído por um utilizador a um artigo.
    // </summary>
    public class ItemLike
    {
        // <summary>
        // PK
        // </summary>
        [Key]
        public int Id { get; set; }

        // <summary>
        // FK para o item (Item)
        // </summary>
        public int ItemId { get; set; }

        // <summary>
        // Item associado a este like.
        // </summary>
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        // <summary>
        // Id do utilizador que deu like (MyUser)
        // </summary>
        public int UserId { get; set; }

        // <summary>
        // Utilizador associado a este like.
        // </summary>
        [ForeignKey(nameof(UserId))]
        public MyUser User { get; set; } = null!;


        // <summary>
        // Data e hora em que o like foi criado.
        // </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
