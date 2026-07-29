namespace Ecosystem_backend.DTOs
{
    public class RegistroGastoDto
    {
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? IdProveedor { get; set; } // Nulo por si es un gasto diferente
        public string Concepto { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class ActualizarGastoDto
    {
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? IdProveedor { get; set; } // Nulo por si es un gasto diferente
        public string Concepto { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}