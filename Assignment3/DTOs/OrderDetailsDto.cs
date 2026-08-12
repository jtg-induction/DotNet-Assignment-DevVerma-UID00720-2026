using System;
using System.Collections.Generic;

namespace Assignment3.DTOs
{
    public class OrderDetailsDto
    {
        public long OrderId { get; set; }
        public string Status { get; set; }
        public string RestaurantName { get; set; }
        public decimal TotalAmount { get; set; }

        public string BuildingNumber { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemDetailsDto> Items { get; set; }
    }
}
