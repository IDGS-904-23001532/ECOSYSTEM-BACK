using System.ComponentModel.DataAnnotations;

namespace Ecosystem_backend.Models
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Contacto { get; set; } = string.Empty;

        public string? Informacion { get; set; }
    }
}
