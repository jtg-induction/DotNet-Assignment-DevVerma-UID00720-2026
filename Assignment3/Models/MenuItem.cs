using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Assignment3.Models
{
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int Stock { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; }

        public MenuItem()
        {
            OrderItems = new HashSet<OrderItem>();
        }
    }
}