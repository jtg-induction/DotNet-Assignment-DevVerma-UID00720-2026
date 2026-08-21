using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
