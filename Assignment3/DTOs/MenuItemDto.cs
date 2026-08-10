namespace Assignment3.DTOs
{
    public class MenuItemDto
    {
        public long Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int Stock {  get; set; }
    }
}
