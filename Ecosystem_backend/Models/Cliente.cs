using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecosystem_backend.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        // Enlazamos al prospecto del cual nació este cliente
        public int? IdProspecto { get; set; }
        [ForeignKey("IdProspecto")]
        public Prospecto? ProspectoOrigen { get; set; }

        [Required]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        // Mismos campos y reglas que Prospecto
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        public string? Corporativo { get; set; }

        [Required]
        public string Localidad { get; set; } = string.Empty;

        // Dato exclusivo del cliente
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

    }
}
