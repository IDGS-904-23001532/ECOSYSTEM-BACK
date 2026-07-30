using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecosystem_backend.Data;
using Ecosystem_backend.Models;
using Ecosystem_backend.DTOs; // Ajusta este namespace según la carpeta donde pongas tus DTOs

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentaController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LISTAR TODAS LAS VENTAS
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/venta
         * ¿Qué enviar?: Nada (No requiere Body).
         * ¿Qué devuelve?: Un arreglo con el historial de ventas, incluyendo el nombre completo del cliente.
         * 
         * Ejemplo de respuesta:
         * [
         *   {
         *     "idVenta": 1,
         *     "idCliente": 5,
         *     "clienteNombre": "María López",
         *     "fecha": "2026-05-16T10:00:00",
         *     "total": 25000.50,
         *     "metodoPago": "Transferencia",
         *     "estado": "Pendiente"
         *   }
         * ]
         */
        [HttpGet]
        public async Task<IActionResult> GetVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Select(v => new
                {
                    v.IdVenta,
                    v.IdCliente,
                    ClienteNombre = v.Cliente != null ? v.Cliente.Nombre + " " + v.Cliente.Apellido : "Desconocido",
                    v.Fecha,
                    v.Total,
                    v.Descripcion,
                    v.MetodoPago,
                    v.Estado
                })
                .OrderByDescending(v => v.Fecha) // Ordenamos de la más reciente a la más antigua
                .ToListAsync();

            return Ok(ventas);
        }

        // ==========================================
        // 2. VER DETALLE DE UNA VENTA
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/venta/{id}
         * ¿Qué enviar?: El ID de la venta en la URL.
         * ¿Qué devuelve?: Toda la información de la venta, incluyendo los datos del Cliente asociado.
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada." });

            return Ok(venta);
        }

        // ==========================================
        // 3. CREAR VENTA DIRECTA (MANUAL)
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: POST
         * Ruta: /api/venta
         * 
         * ¿Qué enviar? (JSON Body):
         * {
         *   "idCliente": 2,
         *   "total": 5000.00,
         *   "descripcion": "Venta directa de refacciones",
         *   "metodoPago": "Efectivo"
         * }
         * 
         * Uso: Este endpoint sirve por si se hace una venta que NO viene de una cotización (venta directa a un cliente que ya existe).
         * Por defecto, la crea con estado "Pendiente".
         */
        [HttpPost]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaDto request)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == request.IdCliente);
            if (!clienteExiste)
                return BadRequest(new { mensaje = "El cliente seleccionado no existe." });

            var nuevaVenta = new Venta
            {
                IdCliente = request.IdCliente,
                Fecha = DateTime.Now,
                Total = request.Total,
                Descripcion = request.Descripcion,
                MetodoPago = request.MetodoPago,
                Estado = "Pendiente" // Se puede cambiar según la lógica del negocio
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVenta), new { id = nuevaVenta.IdVenta }, nuevaVenta);
        }

        // ==========================================
        // 4. CONFIRMAR PAGO / COMPLETAR VENTA
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: PUT
         * Ruta: /api/venta/{id}/completar
         * ¿Qué enviar?: Solo el ID en la URL.
         * Uso: Cambia el estado de la venta de "Pendiente" a "Completo". Ideal para cuando administración verifica que cayó el depósito.
         */
        [HttpPut("{id}/completar")]
        public async Task<IActionResult> CompletarVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada." });

            if (venta.Estado == "Completo")
                return BadRequest(new { mensaje = "Esta venta ya ha sido marcada como completa anteriormente." });

            venta.Estado = "Completo";

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Venta marcada como completa (pagada) exitosamente.", venta });
        }

        // ==========================================
        // 5. ELIMINAR / CANCELAR VENTA
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: DELETE
         * Ruta: /api/venta/{id}
         * ¿Qué enviar?: Solo el ID en la URL.
         * Uso: Elimina físicamente el registro de la venta en caso de error.
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                return NotFound(new { mensaje = "Venta no encontrada." });

            // Dependiendo de las reglas de negocio, tal vez no se deba eliminar si ya está "Completa"
            if (venta.Estado == "Completo")
                return BadRequest(new { mensaje = "No se puede eliminar una venta que ya ha sido completada." });

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Venta eliminada correctamente." });
        }
    }

    // ==========================================
    // DTOs (Data Transfer Objects)
    // ==========================================

    public class CrearVentaDto
    {
        public int IdCliente { get; set; }
        public decimal Total { get; set; }
        public string? Descripcion { get; set; }
        public string MetodoPago { get; set; } = "Transferencia";
    }
}