namespace Ecosystem_backend.DTOs
{
    public class ActualizarClienteDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        public string? Corporativo { get; set; }
    }
}
