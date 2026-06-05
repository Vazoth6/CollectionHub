using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Models.DTOs;

public class TransactionReadDTO
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string? SellerName { get; set; }
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TransactionCreateDTO
{
    [Required]
    public int SellerId { get; set; }

    [Required]
    public int BuyerId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(200)]
    public string ShippingAddress { get; set; } = string.Empty;

    public string Status { get; set; } = "Concluída";
}

public class TransactionUpdateDTO : TransactionCreateDTO
{
}
