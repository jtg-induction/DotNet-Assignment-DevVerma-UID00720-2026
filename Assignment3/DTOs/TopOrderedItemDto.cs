namespace Assignment3.DTOs
{
    public class TopOrderedItemDto
    {
        public long MenuItemId { get; set; }

        public string MenuItemName { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; }

        public int OrderCount { get; set; }
    }
}
