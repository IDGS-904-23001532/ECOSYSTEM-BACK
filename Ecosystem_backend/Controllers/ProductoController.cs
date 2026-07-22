using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecosystem_backend.Data;
using Ecosystem_backend.Models;
using Ecosystem_backend.DTOs;

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos el contexto de la base de datos
        public ProductoController(AppDbContext context)
        {
            _context = context;
        }
        // 1. Crear un nuevo producto (Sistema de Gestion - Productos)
        // http://localhost:5048/api/Producto/registrar-producto
        /*
         * POST
         * {
          "Nombre": "Panel Solar 300W",
        "Descripcion": "Panel solar de alta eficiencia para sistemas residenciales y comerciales.",
        "Precio": 250.00,
        "RutaImagen": "https://example.com/images/panel_solar_300w.jpg"
        }
         */
        [HttpPost("registrar-producto")]
        public async Task<IActionResult> RegistrarProducto([FromBody] RegistroProductoDto request)
        {
            // Validamos que el producto no exista ya (por nombre)
            var existeProducto = await _context.Productos.AnyAsync(p => p.Nombre == request.Nombre);
            if (existeProducto)
            {
                return BadRequest(new { Mensaje = "El producto ya está registrado." });
            }

            // Creamos el nuevo producto
            var nuevoProducto = new Producto
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                RutaImagen = request.RutaImagen
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Producto registrado con éxito", producto = nuevoProducto });
        }

        // 2/ Listar todos los productos (Sistema de Gestion - Productos)
        // http://localhost:5048/api/Producto/listar-productos
        /*
         * GET
         */
        [HttpGet("listar-productos")]
        public async Task<IActionResult> ListarProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(productos);
        }
    }
}