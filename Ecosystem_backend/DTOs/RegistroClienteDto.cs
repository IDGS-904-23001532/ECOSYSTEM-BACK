namespace Ecosystem_backend.DTOs
{
    public class RegistroClienteDto
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? DireccionInstalacion { get; set; }
        public string? Telefono { get; set; }
    }
}
