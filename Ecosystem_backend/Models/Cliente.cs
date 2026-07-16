using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCliente { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? DireccionInstalacion { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        // Propiedad de navegación (Relación 1:1)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}
