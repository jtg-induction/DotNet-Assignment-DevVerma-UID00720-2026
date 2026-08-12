using Assignment3.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class CreateOrderRequestDto
{
    [Required]
    public int RestaurantId { get; set; }

    [Required]
    public long AddressId { get; set; }

    [Required]
    public List<CreateOrderItemDto> Items { get; set; }
}
