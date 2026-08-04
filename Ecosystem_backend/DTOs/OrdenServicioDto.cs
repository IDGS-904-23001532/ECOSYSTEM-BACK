namespace Ecosystem_backend.DTOs
{
    public class CrearOrdenDto
    {
        public int IdCliente { get; set; }
        public DateTime FechaProgramada { get; set; }
        public string DetalleManual { get; set; } = string.Empty;
    }

    public class ActualizarEstatusOrdenDto
    {
        public string NuevoEstatus { get; set; } = string.Empty;
    }
}
