using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class UpdatePasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
