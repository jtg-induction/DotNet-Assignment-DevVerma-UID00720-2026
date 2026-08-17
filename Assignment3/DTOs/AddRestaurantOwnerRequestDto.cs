using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class AddRestaurantOwnerRequestDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        [StringLength(100)]
        public string OwnerName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string OwnerEmail { get; set; }

        [Required]
        [StringLength(255)]
        public string OwnerPassword { get; set; }
    }
}
