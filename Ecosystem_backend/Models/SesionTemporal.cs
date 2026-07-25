using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    [Table("SesionesTemporales")]
    public class SesionTemporal
    {
        [Key]
        public int IdSesion { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public string TokenJWT { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaExpiracion { get; set; }
    }
}
