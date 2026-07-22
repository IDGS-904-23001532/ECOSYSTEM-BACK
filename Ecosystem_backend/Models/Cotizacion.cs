    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Ecosystem_backend.Models
{
    [Table("Cotizaciones")]
        public class Cotizacion
        {
            [Key]
            public int IdCotizacion { get; set; }

            [Required]
            public int IdProspecto { get; set; }
            [ForeignKey("IdProspecto")]
            public Prospecto? Prospecto { get; set; }

            public DateTime FechaEmision { get; set; } = DateTime.Now;

            [Required]
            public decimal TotalCotizado { get; set; }

            [Required]
            public string Estatus { get; set; } = "Pendiente"; // Pendiente, Aceptada, Rechazada
        }
    }
