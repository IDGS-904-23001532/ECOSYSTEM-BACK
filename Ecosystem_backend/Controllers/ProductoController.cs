using Ecosystem_backend.Data;
using Ecosystem_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductoController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Método auxiliar seguro para obtener la ruta webroot sin que explote en producción si es null
        private string GetWebRootPath()
        {
            if (!string.IsNullOrEmpty(_env.WebRootPath))
            {
                return _env.WebRootPath;
            }
            // Fallback seguro para contenedores en la nube (Railway/Docker) donde wwwroot no viene precreado
            var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(fallbackPath))
            {
                Directory.CreateDirectory(fallbackPath);
            }
            return fallbackPath;
        }

        // GET: api/<ProductoController>
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            try
            {
                var list_productos = await _context.Productos.ToListAsync();
                return Ok(list_productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener productos.", details = ex.Message });
            }
        }

        // GET api/<ProductoController>/5
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByNameProduct(string name)
        {
            try
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == name);
                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." });
                }
                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar el producto.", details = ex.Message });
            }
        }

        // POST api/<ProductoController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Producto product, IFormFile? routeFile)
        {
            try
            {
                var product_exists = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == product.Nombre);
                if (product_exists != null)
                {
                    return BadRequest("Este producto ya existe con el mismo nombre.");
                }

                // Manejo seguro del archivo
                if (routeFile != null && routeFile.Length > 0)
                {
                    string webRootPath = GetWebRootPath();
                    string carpeta_destino = Path.Combine(webRootPath, "Uploads");

                    if (!Directory.Exists(carpeta_destino))
                    {
                        Directory.CreateDirectory(carpeta_destino);
                    }

                    string extension = Path.GetExtension(routeFile.FileName);
                    string nombre_unico = Guid.NewGuid().ToString() + extension;
                    string ruta_fisica = Path.Combine(carpeta_destino, nombre_unico);

                    using (var stream = new FileStream(ruta_fisica, FileMode.Create))
                    {
                        await routeFile.CopyToAsync(stream);
                    }

                    product.RutaImagen = "/Uploads/" + nombre_unico;
                }
                else if (string.IsNullOrEmpty(product.RutaImagen))
                {
                    product.RutaImagen = "/Uploads/default.png"; // Valor por defecto si no mandan imagen
                }

                _context.Productos.Add(product);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (DbUpdateException dbEx)
            {
                // Muestra detalles de la base de datos (ej. restricciones o campos nulos)
                return StatusCode(500, new { message = "Error de base de datos al guardar.", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor.", details = ex.Message });
            }
        }

        // PUT api/<ProductoController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] Producto product, IFormFile? routeFile)
        {
            if (product == null || (id != product.IdProducto && product.IdProducto != 0))
            {
                return BadRequest("Los datos enviados son inválidos.");
            }

            try
            {
                var product_exists = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
                if (product_exists == null)
                {
                    return NotFound("No se ha encontrado ninguna coincidencia para actualizar.");
                }

                if (routeFile != null && routeFile.Length > 0)
                {
                    string webRootPath = GetWebRootPath();

                    if (!string.IsNullOrEmpty(product_exists.RutaImagen))
                    {
                        string ruta_anterior = Path.Combine(webRootPath, product_exists.RutaImagen.TrimStart('/'));
                        if (System.IO.File.Exists(ruta_anterior))
                        {
                            System.IO.File.Delete(ruta_anterior);
                        }
                    }

                    string carpeta_destino = Path.Combine(webRootPath, "Uploads");
                    if (!Directory.Exists(carpeta_destino))
                    {
                        Directory.CreateDirectory(carpeta_destino);
                    }

                    string extension = Path.GetExtension(routeFile.FileName);
                    string nombre_unico = Guid.NewGuid().ToString() + extension;
                    string ruta_fisica = Path.Combine(carpeta_destino, nombre_unico);

                    using (var stream = new FileStream(ruta_fisica, FileMode.Create))
                    {
                        await routeFile.CopyToAsync(stream);
                    }

                    product_exists.RutaImagen = "/Uploads/" + nombre_unico;
                }

                product_exists.Nombre = product.Nombre;
                product_exists.Descripcion = product.Descripcion;
                product_exists.Precio = product.Precio;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto actualizado correctamente." });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = "Error de base de datos al actualizar.", details = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor.", details = ex.Message });
            }
        }

        // DELETE api/<ProductoController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product_exists = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
                if (product_exists == null)
                {
                    return NotFound("El producto que intentas eliminar no existe.");
                }

                if (!string.IsNullOrEmpty(product_exists.RutaImagen))
                {
                    string webRootPath = GetWebRootPath();
                    string ruta_archivo = Path.Combine(webRootPath, product_exists.RutaImagen.TrimStart('/'));

                    if (System.IO.File.Exists(ruta_archivo))
                    {
                        System.IO.File.Delete(ruta_archivo);
                    }
                }

                _context.Productos.Remove(product_exists);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto y archivos eliminados correctamente." });
            }
            catch (DbUpdateException dbEx)
            {
                // Esto te dirá exactamente si el borrado falló por una Llave Foránea (Foreign Key)
                return StatusCode(500, new
                {
                    message = "No se puede eliminar el producto porque está vinculado a otros registros en el sistema.",
                    details = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor.", details = ex.Message });
            }
        }
    }
}