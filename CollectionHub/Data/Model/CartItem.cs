namespace CollectionHub.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;

        public decimal Subtotal => Price * Quantity;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}