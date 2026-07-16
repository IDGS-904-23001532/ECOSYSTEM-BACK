namespace Ecosystem_backend.DTOs
{
    public class RegistroEmpleadoDto
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Puesto { get; set; }
    }
}
