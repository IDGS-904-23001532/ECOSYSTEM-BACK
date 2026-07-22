namespace Ecosystem_backend.DTOs
{

    public class DetalleCotizacionDto
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }

    public class RegistroCotizacionDto
    {
        public int IdProspecto { get; set; }
        public List<DetalleCotizacionDto> Detalles { get; set; } = new List<DetalleCotizacionDto>();
    }

    public class CierreCotizacionDto
    {
        public string Accion { get; set; } = string.Empty; // "aceptar" o "rechazar"
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    }
