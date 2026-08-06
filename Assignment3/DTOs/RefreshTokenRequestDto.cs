using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
