using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class AddRestaurantOwnerRequestDto
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        public List<RestaurantOwnerDto> Owners { get; set; }
    }
}
