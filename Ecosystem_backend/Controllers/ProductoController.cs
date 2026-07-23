using Ecosystem_backend.Data;
using Ecosystem_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Contexto de la base de datos
        public ProductoController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/<ProductoController>
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var list_productos = await _context.Productos.ToListAsync();

            return Ok(list_productos);
        }

        // GET api/<ProductoController>/5
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByNameProduct(string name)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == name);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        // POST api/<ProductoController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Producto product, IFormFile routeFile)
        {
            try
            {
                var product_exists = await _context.Productos.FirstOrDefaultAsync(p => p.Nombre == product.Nombre);

                if (product_exists != null)
                {
                    return BadRequest("Este producto ya existe");
                }

                if (routeFile != null && routeFile.Length > 0)
                {
                    string carpeta_destino = Path.Combine(_env.WebRootPath, "Uploads");

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

                _context.Productos.Add(product);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception e)
            {
                return StatusCode(500, "Error interno del servidor, consulte con el soporte técnico");
            }
        }

        // PUT api/<ProductoController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] Producto product, IFormFile routeFile)
        {
            if (product == null || id != product.IdProducto)
            {
                return BadRequest("Los datos enviados son invalidos");
            }


            try
            {
                var product_exists = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);

                if (product_exists == null)
                {
                    return NotFound("No se ha encontrado ninguna coincidencia");
                }

                if (routeFile != null && routeFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product_exists.RutaImagen))
                    {
                        string ruta_anterior = Path.Combine(_env.WebRootPath, product_exists.RutaImagen.TrimStart('/'));
                        if (System.IO.File.Exists(ruta_anterior))
                        {
                            System.IO.File.Delete(ruta_anterior);
                        }
                    }

                    string carpeta_destino = Path.Combine(_env.WebRootPath, "Uploads");
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

                return Ok("Producto actualizado correctamente");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Error interno del servidor, consulte con el soporte técnico");
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
                    string ruta_archivo = Path.Combine(_env.WebRootPath, product_exists.RutaImagen.TrimStart('/'));

                    if (System.IO.File.Exists(ruta_archivo))
                    {
                        System.IO.File.Delete(ruta_archivo);
                    }
                }

                _context.Productos.Remove(product_exists);

                await _context.SaveChangesAsync();

                return Ok("Producto y archivos eliminados correctamente.");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Error interno del servidor, consulte con el soporte técnico");
            }
        }
    }
}