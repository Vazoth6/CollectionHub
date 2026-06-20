using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = "Utilizador";

        [Display(Name = "Telemóvel")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Formato de telemóvel inválido")]
        public string? CellPhone { get; set; }

        public DateTime RegisterDate { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string UserID { get; set; } = "";

        public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

        public ICollection<Transaction> Sales { get; set; } = new List<Transaction>();

        public ICollection<Transaction> Purchases { get; set; } = new List<Transaction>();

        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
    }
}