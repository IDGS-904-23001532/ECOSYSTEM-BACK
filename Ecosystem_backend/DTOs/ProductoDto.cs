namespace Ecosystem_backend.DTOs
{
    public class RegistroProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string RutaImagen { get; set; } = string.Empty;
    }
}
