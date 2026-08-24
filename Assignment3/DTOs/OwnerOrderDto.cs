using System;

namespace Assignment3.DTOs
{
    public class OwnerOrderDto
    {
        public long OrderId { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; }

        public long UserId { get; set; }

        public string Status { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
