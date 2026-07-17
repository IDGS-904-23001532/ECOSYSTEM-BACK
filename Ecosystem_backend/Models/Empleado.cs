using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    [Table("Empleados")]
    public class Empleado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEmpleado { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Puesto { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // Propiedad de navegación (Relación 1:1)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}
