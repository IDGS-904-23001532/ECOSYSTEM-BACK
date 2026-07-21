using System.ComponentModel.DataAnnotations;

namespace Ecosystem_backend.Models
{
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; // Panel, Inversor, Cable

        public string? Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        // Guarda la ruta (ej. "/images/productos/panel-solar.jpg")
        public string? RutaImagen { get; set; }
    }
}
