using System.ComponentModel.DataAnnotations;

namespace Ecosystem_backend.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
