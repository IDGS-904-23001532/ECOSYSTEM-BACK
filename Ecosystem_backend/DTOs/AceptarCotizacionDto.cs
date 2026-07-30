namespace Ecosystem_backend.DTOs
{
    public class AceptarCotizacionDto
    {
        public string MetodoPago { get; set; } = "Transferencia";
        public string? Descripcion { get; set; }
    }
}
