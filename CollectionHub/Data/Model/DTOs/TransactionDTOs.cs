using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    /// <summary>
    /// DTO para criar uma nova transação (compra)
    /// </summary>
    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "O ID do item é obrigatório")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "O endereço de entrega é obrigatório")]
        [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres")]
        [Display(Name = "Endereço de Entrega")]
        public string ShippingAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualizar o status de uma transação
    /// </summary>
    public class UpdateTransactionStatusDto
    {
        [Required(ErrorMessage = "O status é obrigatório")]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de transação (lista)
    /// </summary>
    public class TransactionListItemDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string OtherPartyName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // "Compra" ou "Venda"
    }

    /// <summary>
    /// DTO para resposta de lista de transações
    /// </summary>
    public class TransactionListResponseDto
    {
        public List<TransactionListItemDto> Purchases { get; set; } = new();
        public List<TransactionListItemDto> Sales { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de transação (detalhe)
    /// </summary>
    public class TransactionDetailResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TransactionPartyDto? Item { get; set; }
        public TransactionPartyDto? Seller { get; set; }
        public TransactionPartyDto? Buyer { get; set; }
    }

    public class TransactionPartyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO para estatísticas de transações
    /// </summary>
    public class TransactionStatsDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public int SalesCount { get; set; }
        public int PurchasesCount { get; set; }
    }
}