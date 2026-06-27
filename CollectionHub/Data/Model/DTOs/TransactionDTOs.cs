using System.ComponentModel.DataAnnotations;

namespace CollectionHub.Data.Model.DTOs
{
    // <summary>
    // DTO para criar uma nova transação (compra)
    // </summary>
    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "O ID do item é obrigatório")]
        [Display(Name = "Item")]
        // <summary>
        // Obtém ou define item id.
        // </summary>
        public int ItemId { get; set; }

        [Required(ErrorMessage = "O endereço de entrega é obrigatório")]
        [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres")]
        [Display(Name = "Endereço de Entrega")]
        // <summary>
        // Obtém ou define endereço de entrega.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para actualizar o estado de uma transação
    // </summary>
    public class UpdateTransactionStatusDto
    {
        [Required(ErrorMessage = "O estado é obrigatório")]
        [Display(Name = "Estado")]
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
    }

    // <summary>
    // DTO para resposta de transação (lista)
    // </summary>
    public class TransactionListItemDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define data.
        // </summary>
        public DateTime Date { get; set; }
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define endereço de entrega.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome do item.
        // </summary>
        public string ItemName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define nome da outra parte.
        // </summary>
        public string OtherPartyName { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define tipo de transação.
        // </summary>
        public string TransactionType { get; set; } = string.Empty; // "Compra" ou "Venda"
    }

    // <summary>
    // DTO para resposta de lista de transações
    // </summary>
    public class TransactionListResponseDto
    {
        // <summary>
        // Obtém ou define compras.
        // </summary>
        public List<TransactionListItemDto> Purchases { get; set; } = new();
        // <summary>
        // Obtém ou define vendas.
        // </summary>
        public List<TransactionListItemDto> Sales { get; set; } = new();
    }

    // <summary>
    // DTO para resposta de transação (detalhes)
    // </summary>
    public class TransactionDetailResponseDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define data.
        // </summary>
        public DateTime Date { get; set; }
        // <summary>
        // Obtém ou define preço.
        // </summary>
        public decimal Price { get; set; }
        // <summary>
        // Obtém ou define endereço de entrega.
        // </summary>
        public string ShippingAddress { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define estado.
        // </summary>
        public string Status { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define artigo.
        // </summary>
        public TransactionPartyDto? Item { get; set; }
        // <summary>
        // Obtém ou define vendedor.
        // </summary>
        public TransactionPartyDto? Seller { get; set; }
        // <summary>
        // Obtém ou define comprador.
        // </summary>
        public TransactionPartyDto? Buyer { get; set; }
    }

    // <summary>
    // Representa os dados transferidos entre a interface/API e a aplicação para transaction party dto.
    // </summary>
    public class TransactionPartyDto
    {
        // <summary>
        // Obtém ou define id.
        // </summary>
        public int Id { get; set; }
        // <summary>
        // Obtém ou define nome.
        // </summary>
        public string Name { get; set; } = string.Empty;
        // <summary>
        // Obtém ou define descrição.
        // </summary>
        public string? Description { get; set; }
    }

    // <summary>
    // DTO para estatísticas de transações
    // </summary>
    public class TransactionStatsDto
    {
        // <summary>
        // Obtém ou define total de vendas.
        // </summary>
        public decimal TotalSales { get; set; }
        // <summary>
        // Obtém ou define total de compras.
        // </summary>
        public decimal TotalPurchases { get; set; }
        // <summary>
        // Obtém ou define contagem de vendas.
        // </summary>
        public int SalesCount { get; set; }
        // <summary>
        // Obtém ou define contagem de compras.
        // </summary>
        public int PurchasesCount { get; set; }
    }
}
