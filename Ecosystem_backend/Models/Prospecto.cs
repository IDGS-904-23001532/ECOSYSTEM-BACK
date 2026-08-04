using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    [Table("Prospectos")]
    public class Prospecto
    {
        [Key]
        public int IdProspecto { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        public string? Corporativo { get; set; }

        [Required]
        public string Localidad { get; set; } = string.Empty;

        [Required]
        public string Estatus { get; set; } = "Pendiente"; // Pendiente, Aceptado, Cancelado

    }
}
