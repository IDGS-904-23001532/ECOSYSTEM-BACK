using System.ComponentModel.DataAnnotations;

namespace Ecosystem_backend.DTOs
{
    public class ProveedorDto
    {
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto del proveedor es obligatorio.")]
        public string Contacto { get; set; } = string.Empty;

        public string? Informacion { get; set; }
    }
}
