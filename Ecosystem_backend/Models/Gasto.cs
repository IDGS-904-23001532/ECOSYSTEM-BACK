using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    public class Gasto
    {
        [Key]
        public int IdGasto { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int? IdProveedor { get; set; } // Nulo por si es un gasto diferente
        [ForeignKey("IdProveedor")]
        public Proveedor? Proveedor { get; set; }

        [Required]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        public decimal Total { get; set; }
    }
}
