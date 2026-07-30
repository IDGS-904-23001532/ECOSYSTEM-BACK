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
    public class OrdenServicioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdenServicioController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LISTAR TODAS LAS ÓRDENES DE SERVICIO
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/ordenservicio
         * ¿Qué enviar?: Nada (No requiere Body).
         * ¿Qué devuelve?: Un arreglo con todas las órdenes, incluyendo el nombre del cliente asociado.
         * 
         * Ejemplo de respuesta:
         * [
         *   {
         *     "idOrden": 1,
         *     "idCliente": 3,
         *     "clienteNombre": "Juan Pérez",
         *     "fechaProgramada": "2026-08-15T14:30:00",
         *     "detalleManual": "Falla en el inversor del panel sur",
         *     "estatus": "Pendiente"
         *   }
         * ]
         */
        [HttpGet]
        public async Task<IActionResult> GetOrdenes()
        {
            var ordenes = await _context.OrdenesServicio
                .Include(o => o.Cliente)
                .Select(o => new
                {
                    o.IdOrden,
                    o.IdCliente,
                    ClienteNombre = o.Cliente != null ? o.Cliente.Nombre + " " + o.Cliente.Apellido : "Desconocido",
                    o.FechaProgramada,
                    o.DetalleManual,
                    o.Estatus
                })
                .OrderBy(o => o.FechaProgramada) // Ordenamos por fecha para atender primero las más próximas
                .ToListAsync();

            return Ok(ordenes);
        }

        // ==========================================
        // 2. VER DETALLE DE UNA ORDEN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/ordenservicio/{id}
         * ¿Qué enviar?: El ID de la orden en la URL.
         * ¿Qué devuelve?: Toda la información de esa orden específica.
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrden(int id)
        {
            var orden = await _context.OrdenesServicio
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.IdOrden == id);

            if (orden == null)
                return NotFound(new { mensaje = "Orden de servicio no encontrada." });

            return Ok(new
            {
                orden.IdOrden,
                orden.IdCliente,
                ClienteNombre = orden.Cliente != null ? orden.Cliente.Nombre + " " + orden.Cliente.Apellido : "Desconocido",
                orden.FechaProgramada,
                orden.DetalleManual,
                orden.Estatus
            });
        }

        // ==========================================
        // 3. CREAR NUEVA ORDEN DE SERVICIO
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: POST
         * Ruta: /api/ordenservicio
         * 
         * ¿Qué enviar? (JSON Body):
         * {
         *   "idCliente": 3,
         *   "fechaProgramada": "2026-08-15T14:30:00",
         *   "detalleManual": "Revisión de cableado y falla en inversor"
         * }
         * 
         * Uso: Crea la orden automáticamente con estatus "Pendiente".
         */
        [HttpPost]
        public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenDto request)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == request.IdCliente);
            if (!clienteExiste)
                return BadRequest(new { mensaje = "El cliente seleccionado no existe." });

            var nuevaOrden = new OrdenServicio
            {
                IdCliente = request.IdCliente,
                FechaProgramada = request.FechaProgramada,
                DetalleManual = request.DetalleManual,
                Estatus = "Pendiente" // Se inicializa por defecto
            };

            _context.OrdenesServicio.Add(nuevaOrden);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrden), new { id = nuevaOrden.IdOrden }, nuevaOrden);
        }

        // ==========================================
        // 4. ACTUALIZAR ESTATUS DE LA ORDEN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: PUT
         * Ruta: /api/ordenservicio/{id}/estatus
         * 
         * ¿Qué enviar? (JSON Body):
         * {
         *   "nuevoEstatus": "Completada" // Puede ser "En Proceso", "Completada", "Cancelada", etc.
         * }
         * 
         * Uso: Para que los técnicos o administración marquen cuando ya atendieron el reporte.
         */
        [HttpPut("{id}/estatus")]
        public async Task<IActionResult> ActualizarEstatus(int id, [FromBody] ActualizarEstatusOrdenDto request)
        {
            var orden = await _context.OrdenesServicio.FindAsync(id);

            if (orden == null)
                return NotFound(new { mensaje = "Orden de servicio no encontrada." });

            if (string.IsNullOrWhiteSpace(request.NuevoEstatus))
                return BadRequest(new { mensaje = "El estatus no puede estar vacío." });

            orden.Estatus = request.NuevoEstatus;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Estatus actualizado a '{request.NuevoEstatus}' exitosamente.", orden });
        }

        // ==========================================
        // 5. ELIMINAR ORDEN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: DELETE
         * Ruta: /api/ordenservicio/{id}
         * ¿Qué enviar?: Solo el ID en la URL.
         * Uso: Elimina físicamente el registro (ideal para errores de captura).
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarOrden(int id)
        {
            var orden = await _context.OrdenesServicio.FindAsync(id);

            if (orden == null)
                return NotFound(new { mensaje = "Orden de servicio no encontrada." });

            _context.OrdenesServicio.Remove(orden);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Orden de servicio eliminada correctamente." });
        }
    }

}