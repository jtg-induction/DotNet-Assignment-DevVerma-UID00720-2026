using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment3.Models
{
    [Table("Addresses")]
    public class Address
    {
        [Key]
        public long Id { get; set; }

        public long? UserId { get; set; }

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

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}