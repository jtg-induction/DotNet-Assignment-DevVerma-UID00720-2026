using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class AddRestaurantRequestDto
    {
        // Restaurant
        [Required]
        [StringLength(100)]
        public string RestaurantName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string RestaurantEmail { get; set; }

        // Restaurant Address
        [Required]
        [StringLength(20)]
        public string BuildingNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Locality { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        // Initial owners
        [Required]
        public List<RestaurantOwnerDto> Owners { get; set; }
    }
}
