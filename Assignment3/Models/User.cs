using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public decimal Balance { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<RestaurantOwner> RestaurantOwners { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        public User()
        {
            Addresses = new HashSet<Address>();
            Orders = new HashSet<Order>();
            RestaurantOwners = new HashSet<RestaurantOwner>();
            RefreshTokens = new HashSet<RefreshToken>();
        }
    }
}
