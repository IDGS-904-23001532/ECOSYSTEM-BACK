namespace Ecosystem_backend.DTOs
{
    public class VentaDTO
    {
        public int IdCliente { get; set; }

        public decimal Total { get; set; }

        public string? Descripcion { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string Estado { get; set; } = "Pendiente";
    }
}