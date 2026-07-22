namespace Ecosystem_backend.DTOs
{
    public class RegistroProspectoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Corporativo { get; set; }
        public string Localidad { get; set; } = string.Empty;
    }

    public class ActualizarEstatusDto
	{
        public string Estatus { get; set; } = string.Empty;
    }
}
