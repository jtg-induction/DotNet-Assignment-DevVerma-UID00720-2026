using System.Collections.Generic;

namespace Assignment3.DTOs
{
    public class TopOrderedItemsRequestDto
    {
        public int? RestaurantId { get; set; }

        public List<long> ExcludeMenuItemIds { get; set; }
    }
}
