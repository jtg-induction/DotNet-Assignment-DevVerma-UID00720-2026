namespace Assignment3.DTOs
{
    public class OrderItemDetailsDto
    {
        public long MenuItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
