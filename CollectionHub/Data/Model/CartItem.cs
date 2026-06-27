namespace CollectionHub.Models
{
    // <summary>
    // Representa um item individual no carrinho de compras do utilizador.
    // Contém as informações do produto e a quantidade selecionada.
    // </summary>
    public class CartItem
    {
        // <summary>
        // Id único do item no carrinho.
        // </summary>
        public int Id { get; set; }

        // <summary>
        // Nome do produto adicionado ao carrinho.
        // </summary>
        public string Name { get; set; } = string.Empty;

        // <summary>
        // Preço unitário do produto.
        // </summary>
        public decimal Price { get; set; }

        // <summary>
        // URL da imagem do produto para exibição no carrinho.
        // </summary>
        public string ImageUrl { get; set; } = string.Empty;

        // <summary>
        // Quantidade do produto selecionada pelo utilizador.
        // Valor padrão é 1.
        // </summary>
        public int Quantity { get; set; } = 1;

        // <summary>
        // Id único do vendedor do produto.
        // </summary>
        public int SellerId { get; set; }

        // <summary>
        // Nome do vendedor do produto.
        // </summary>
        public string SellerName { get; set; } = string.Empty;

        // <summary>
        // Subtotal do item (preço unitário x quantidade).
        // Propriedade calculada somente leitura.
        // </summary>
        public decimal Subtotal => Price * Quantity;
    }

    // <summary>
    // Representa o carrinho de compras completo do utilizador.
    // Gerencia a lista de items e fornece totais agregados.
    // </summary>
    public class ShoppingCart
    {
        // <summary>
        // Lista de items presentes no carrinho de compras.
        // </summary>
        public List<CartItem> Items { get; set; } = new();

        // <summary>
        // Valor total do carrinho (soma de todos os subtotais dos items).
        // Propriedade calculada somente leitura.
        // </summary>
        public decimal Total => Items.Sum(i => i.Subtotal);

        // <summary>
        // Quantidade total de items no carrinho (soma de todas as quantidades).
        // Propriedade calculada somente leitura.
        // </summary>
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
