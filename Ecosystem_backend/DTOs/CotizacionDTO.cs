namespace Ecosystem_backend.DTOs
{

    public class DetalleCotizacionDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CrearCotizacionDto
    {
        public int IdProspecto { get; set; }
        public decimal CostoInstalacion { get; set; }
        // El frontend enviará el arreglo de productos aquí
        public List<DetalleCotizacionDto> Detalles { get; set; } = new List<DetalleCotizacionDto>();
    }

    public class CierreCotizacionDto
    {
        public string Accion { get; set; } = string.Empty; // "aceptar" o "rechazar"
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    }
