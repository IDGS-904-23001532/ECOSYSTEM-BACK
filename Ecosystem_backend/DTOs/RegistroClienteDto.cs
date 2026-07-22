namespace Ecosystem_backend.DTOs
{
    public class RegistroClienteDto
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Corporativo { get; set; }
        public string Localidad { get; set; } = string.Empty;
    }
}
