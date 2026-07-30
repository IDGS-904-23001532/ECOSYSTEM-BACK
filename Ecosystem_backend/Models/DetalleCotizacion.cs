using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ecosystem_backend.Models
{
    public class DetalleCotizacion
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        public int IdCotizacion { get; set; }
        [ForeignKey("IdCotizacion")]
        [JsonIgnore]
        public Cotizacion? Cotizacion { get; set; }

        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}
