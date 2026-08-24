using System.ComponentModel.DataAnnotations;

public class CreateOrderItemDto
{
    [Required]
    public long MenuItemId { get; set; }

    [Required]
    [Range(1,int.MaxValue)]
    public int Quantity { get; set; }
}
