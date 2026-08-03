using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long MenuItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
    }
}