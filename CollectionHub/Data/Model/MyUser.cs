using System.ComponentModel.DataAnnotations;
using System.Net.ServerSentEvents;

namespace CollectionHub.Data.Model
{
    public class MyUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name= "Nome")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public string Role { get; set; } = string.Empty;

        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Relacionamentos
        /// </summary>
        public ICollection<Transaction> Sales { get; set; } = new List<Transaction>();

        //public ICollection<Transaction> Purchases { get; set; } = new List<Transaction>();

        //public ICollection<UserItem> UserItems { get; set; } = new List<UserItem>();

    }
    /// <summary>
    /// Display(Name= "Nome")]
    ///[DataType(data_type)]
    ///[ForeignKey(nameof(fk__reference_variable))]
    ///[StringLength(x)]
    /// </summary>
    /// <param name=""></param>

}
