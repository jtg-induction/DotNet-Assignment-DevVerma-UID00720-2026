using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    [Table("RestaurantOwners")]
    public class RestaurantOwner
    {
        public long UserId { get; set; }

        public int RestaurantId { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; }
    }
}
