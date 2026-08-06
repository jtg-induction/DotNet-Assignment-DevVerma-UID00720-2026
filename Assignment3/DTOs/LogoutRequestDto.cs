using System.ComponentModel.DataAnnotations;

namespace Assignment3.DTOs
{
    public class LogoutRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
