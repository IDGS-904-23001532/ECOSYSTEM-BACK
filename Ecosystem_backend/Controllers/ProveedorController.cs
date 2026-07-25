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
    public class ProveedorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearProveedor([FromBody] ProveedorDto request)
        {
            var nuevoProveedor = new Proveedor
            {
                Nombre = request.Nombre,
                Contacto = request.Contacto,
                Informacion = request.Informacion
            };

            _context.Proveedores.Add(nuevoProveedor);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Proveedor registrado con éxito", IdProveedor = nuevoProveedor.IdProveedor });
        }

        [HttpGet]
        public async Task<IActionResult> ListarProveedores()
        {
            var listaProveedores = await _context.Proveedores.ToListAsync();
            return Ok(listaProveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null) return NotFound("Proveedor no encontrado.");

            return Ok(proveedor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] ProveedorDto request)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null) return NotFound("Proveedor no encontrado.");

            // Actualizamos los valores
            proveedor.Nombre = request.Nombre;
            proveedor.Contacto = request.Contacto;
            proveedor.Informacion = request.Informacion;

            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Proveedor actualizado correctamente" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null) return NotFound("Proveedor no encontrado.");

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Proveedor eliminado permanentemente." });
        }
    }
}