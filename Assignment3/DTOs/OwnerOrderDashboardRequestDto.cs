using System;

namespace Assignment3.DTOs
{
    public class OwnerOrderDashboardRequestDto
    {
        public long? OrderId { get; set; }

        public int? RestaurantId { get; set; }

        public string Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public OwnerOrderDashboardRequestDto()
        {
            SortBy = "createdAt";
            SortOrder = "asc";
            Page = 1;
            PageSize = 10;
        }
    }
}
