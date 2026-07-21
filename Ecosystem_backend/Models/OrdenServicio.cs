using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    public class OrdenServicio
    {
        [Key]
        public int IdOrden { get; set; }

        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        public DateTime FechaProgramada { get; set; }

        [Required]
        public string DetalleManual { get; set; } = string.Empty;

        [Required]
        public string Estatus { get; set; } = "Pendiente";
    }
}
