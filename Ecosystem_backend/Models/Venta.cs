using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public decimal Total { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public string MetodoPago { get; set; } = string.Empty; // Efectivo, Transferencia

        [Required]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Completo
    }
}
