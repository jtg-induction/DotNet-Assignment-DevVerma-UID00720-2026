using System.Collections.Generic;

namespace Assignment3.DTOs
{
    public class OwnerOrderDashboardDto
    {
        public List<OwnerOrderDto> Orders { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }
    }
}
