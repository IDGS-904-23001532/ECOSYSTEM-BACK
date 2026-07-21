using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    public class DetalleCotizacion
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        public int IdCotizacion { get; set; }
        [ForeignKey("IdCotizacion")]
        public Cotizacion? Cotizacion { get; set; }

        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}
