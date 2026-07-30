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
    public class CotizacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CotizacionController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LISTAR TODAS LAS COTIZACIONES
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/cotizacion
         * ¿Qué enviar?: Nada (No requiere Body).
         * ¿Qué devuelve?: Un arreglo de objetos con el resumen de cada cotización. 
         * Ideal para llenar la tabla principal. Incluye el nombre concatenado del prospecto.
         * 
         * Ejemplo de respuesta:
         * [
         *   {
         *     "idCotizacion": 1,
         *     "idProspecto": 3,
         *     "prospectoNombre": "Juan Pérez",
         *     "fechaEmision": "2026-05-15T14:30:00",
         *     "totalCotizado": 25000.50,
         *     "estatus": "Pendiente"
         *   }
         * ]
         */
        [HttpGet]
        public async Task<IActionResult> GetCotizaciones()
        {
            var cotizaciones = await _context.Cotizaciones
                .Include(c => c.Prospecto)
                .Select(c => new
                {
                    c.IdCotizacion,
                    c.IdProspecto,
                    ProspectoNombre = c.Prospecto != null ? c.Prospecto.Nombre + " " + c.Prospecto.Apellido : "Desconocido",
                    c.FechaEmision,
                    c.TotalCotizado,
                    c.Estatus,
                    c.CostoInstalacion,
                    c.Iva
                })
                .ToListAsync();

            return Ok(cotizaciones);
        }

        // ==========================================
        // 2. VER DETALLE COMPLETO DE UNA COTIZACIÓN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: GET
         * Ruta: /api/cotizacion/{id}
         * ¿Qué enviar?: El ID de la cotización en la URL.
         * ¿Qué devuelve?: Toda la info de la cotización, incluyendo los datos completos del Prospecto y el ARREGLO de productos (Detalles).
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCotizacion(int id)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Prospecto)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto) // Incluye la info del catálogo de productos
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);

            if (cotizacion == null)
                return NotFound(new { mensaje = "Cotización no encontrada." });

            return Ok(cotizacion);
        }

        // ==========================================
        // 3. CREAR NUEVA COTIZACIÓN CON PRODUCTOS
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: POST
         * Ruta: /api/cotizacion
         * 
         * ¿Qué enviar? (JSON Body exacto esperado):
         * {
         *   "idProspecto": 1,
         *   "detalles": [
         *     {
         *       "idProducto": 2,
         *       "cantidad": 4,
         *       "subtotal": 10000.00
         *     },
         *     {
         *       "idProducto": 5,
         *       "cantidad": 1,
         *       "subtotal": 5500.50
         *     }
         *   ]
         * }
         * NOTA: El TotalCotizado NO se manda desde el front, el backend lo calcula automáticamente sumando los subtotales para mayor seguridad.
         * 
         * ¿Qué devuelve?: HTTP 201 (Created) con el objeto completo de la cotización recién creada.
         */
        [HttpPost]
        public async Task<IActionResult> CrearCotizacion([FromBody] CrearCotizacionDto request)
        {
            var prospectoExiste = await _context.Prospectos.AnyAsync(p => p.IdProspecto == request.IdProspecto);
            if (!prospectoExiste)
                return BadRequest(new { mensaje = "El prospecto seleccionado no existe en la base de datos." });

            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest(new { mensaje = "La cotización debe incluir al menos un producto." });

            // Obtener los precios y existencia real de los productos desde la base de datos
            var productIds = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productosDb = await _context.Productos
                .Where(p => productIds.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            // Validar que todos los productos solicitados existan
            foreach (var idProd in productIds)
            {
                if (!productosDb.ContainsKey(idProd))
                {
                    return BadRequest(new { mensaje = $"El producto con ID {idProd} no existe en la base de datos." });
                }
            }

            // Calcular subtotales y total general en el backend de forma segura
            decimal totalCalculado = 0;
            var detallesEntidad = new List<DetalleCotizacion>();

            foreach (var d in request.Detalles)
            {
                var producto = productosDb[d.IdProducto];
                decimal subtotal = producto.Precio * d.Cantidad;
                totalCalculado += subtotal;

                detallesEntidad.Add(new DetalleCotizacion
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = subtotal
                });
            }

            decimal ivaCalculado = (totalCalculado + request.CostoInstalacion) * 0.16m;
            decimal totalFinal = totalCalculado + request.CostoInstalacion + ivaCalculado;

            var nuevaCotizacion = new Cotizacion
            {
                IdProspecto = request.IdProspecto,
                CostoInstalacion = request.CostoInstalacion,
                Iva = ivaCalculado,
                TotalCotizado = totalFinal,
                FechaEmision = DateTime.Now,
                Estatus = "Pendiente",
                Detalles = detallesEntidad
            };

            _context.Cotizaciones.Add(nuevaCotizacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCotizacion), new { id = nuevaCotizacion.IdCotizacion }, nuevaCotizacion);
        }

        // ==========================================
        // 4. ELIMINAR COTIZACIÓN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: DELETE
         * Ruta: /api/cotizacion/{id}
         * ¿Qué enviar?: Solo el ID en la URL.
         * ¿Qué devuelve?: Un mensaje de éxito. (EF Core eliminará en cascada los detalles vinculados si está configurado así).
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCotizacion(int id)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(id);

            if (cotizacion == null)
                return NotFound(new { mensaje = "Cotización no encontrada." });

            _context.Cotizaciones.Remove(cotizacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cotización eliminada con éxito." });
        }

        // ==========================================
        // 5. ACEPTAR COTIZACIÓN (FLUJO AUTOMÁTICO)
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: POST
         * Ruta: /api/cotizacion/{idCotizacion}/aceptar
         * 
         * ¿Qué enviar? (JSON Body esperado):
         * {
         *   "metodoPago": "Transferencia", // O "Efectivo"
         *   "descripcion": "Pago inicial del kit solar" // Opcional, si va vacío el sistema pone uno por defecto
         * }
         * 
         * ¿Qué hace el backend?: 
         * 1. Pasa Cotización a "Aceptada".
         * 2. Pasa Prospecto a "Aceptado".
         * 3. Crea al Cliente automáticamente clonando al Prospecto.
         * 4. Registra la Venta vinculada al nuevo cliente.
         * 
         * ¿Qué devuelve?: HTTP 200 con IDs generados.
         * {
         *   "mensaje": "Cotización aceptada...",
         *   "idCliente": 5,
         *   "idVenta": 12
         * }
         */
        [HttpPost("{idCotizacion}/aceptar")]
        public async Task<IActionResult> AceptarCotizacion(int idCotizacion, [FromBody] AceptarCotizacionDto request)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Prospecto)
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null) return NotFound(new { mensaje = "Cotización no encontrada." });
            if (cotizacion.Estatus == "Aceptada") return BadRequest(new { mensaje = "La cotización ya fue aceptada previamente." });
            if (cotizacion.Estatus == "Rechazada") return BadRequest(new { mensaje = "No puedes aceptar una cotización rechazada." });
            if (cotizacion.Prospecto == null) return BadRequest(new { mensaje = "La cotización no tiene un prospecto válido." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                cotizacion.Estatus = "Aceptada";
                cotizacion.Prospecto.Estatus = "Aceptado";

                var nuevoCliente = new Cliente
                {
                    IdProspecto = cotizacion.Prospecto.IdProspecto,
                    Nombre = cotizacion.Prospecto.Nombre,
                    Apellido = cotizacion.Prospecto.Apellido,
                    Telefono = cotizacion.Prospecto.Telefono,
                    Corporativo = cotizacion.Prospecto.Corporativo,
                    Localidad = cotizacion.Prospecto.Localidad,
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                var nuevaVenta = new Venta
                {
                    IdCliente = nuevoCliente.IdCliente,
                    Fecha = DateTime.Now,
                    Total = cotizacion.TotalCotizado,
                    Descripcion = string.IsNullOrWhiteSpace(request.Descripcion)
                                    ? $"Venta automática generada desde cotización #{cotizacion.IdCotizacion}"
                                    : request.Descripcion,
                    MetodoPago = request.MetodoPago,
                    Estado = "Pendiente"
                };

                _context.Ventas.Add(nuevaVenta);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Cotización aceptada, cliente creado y venta registrada con éxito.",
                    idCliente = nuevoCliente.IdCliente,
                    idVenta = nuevaVenta.IdVenta
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al procesar la aceptación.", detalle = ex.Message });
            }
        }

        // ==========================================
        // 6. RECHAZAR COTIZACIÓN
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: POST
         * Ruta: /api/cotizacion/{idCotizacion}/rechazar
         * ¿Qué enviar?: Nada (URL limpia, sin Body).
         * ¿Qué hace el backend?: Pasa la cotización a "Rechazada" y al prospecto a "Cancelado" (baja lógica).
         * ¿Qué devuelve?: Mensaje de éxito.
         */
        [HttpPost("{idCotizacion}/rechazar")]
        public async Task<IActionResult> RechazarCotizacion(int idCotizacion)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Prospecto)
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
                return NotFound(new { mensaje = "Cotización no encontrada." });

            if (cotizacion.Estatus != "Pendiente")
                return BadRequest(new { mensaje = $"No puedes rechazar esta cotización porque actualmente está {cotizacion.Estatus}." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                cotizacion.Estatus = "Rechazada";
                if (cotizacion.Prospecto != null)
                {
                    cotizacion.Prospecto.Estatus = "Cancelado";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Cotización rechazada y prospecto cancelado correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al rechazar la cotización.", detalle = ex.Message });
            }
        }

        // ==========================================
        // 7. ACTUALIZAR COTIZACIÓN (PUT)
        // ==========================================
        /* 
         * FRONTEND INFO:
         * Método: PUT
         * Ruta: /api/cotizacion/{id}
         * 
         * ¿Qué enviar? (JSON Body exacto esperado):
         * {
         *   "idProspecto": 1,
         *   "detalles": [
         *     {
         *       "idProducto": 2,
         *       "cantidad": 3
         *     }
         *   ]
         * }
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCotizacion(int id, [FromBody] CrearCotizacionDto request)
        {
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdCotizacion == id);

            if (cotizacion == null)
                return NotFound(new { mensaje = "Cotización no encontrada." });

            if (cotizacion.Estatus != "Pendiente")
                return BadRequest(new { mensaje = $"Solo se pueden actualizar cotizaciones en estado 'Pendiente'. Estatus actual: {cotizacion.Estatus}." });

            var prospectoExiste = await _context.Prospectos.AnyAsync(p => p.IdProspecto == request.IdProspecto);
            if (!prospectoExiste)
                return BadRequest(new { mensaje = "El prospecto seleccionado no existe en la base de datos." });

            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest(new { mensaje = "La cotización debe incluir al menos un producto." });

            // Obtener los precios y existencia real de los productos desde la base de datos
            var productIds = request.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            var productosDb = await _context.Productos
                .Where(p => productIds.Contains(p.IdProducto))
                .ToDictionaryAsync(p => p.IdProducto);

            // Validar que todos los productos solicitados existan
            foreach (var idProd in productIds)
            {
                if (!productosDb.ContainsKey(idProd))
                {
                    return BadRequest(new { mensaje = $"El producto con ID {idProd} no existe en la base de datos." });
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Limpiar los detalles anteriores (EF Core los marcará como eliminados automáticamente al guardar)
                cotizacion.Detalles.Clear();

                // Calcular subtotales y total general en el backend
                decimal totalCalculado = 0;

                foreach (var d in request.Detalles)
                {
                    var producto = productosDb[d.IdProducto];
                    decimal subtotalVal = producto.Precio * d.Cantidad;
                    totalCalculado += subtotalVal;

                    cotizacion.Detalles.Add(new DetalleCotizacion
                    {
                        IdProducto = d.IdProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalVal
                    });
                }

                decimal ivaCalculado = (totalCalculado + request.CostoInstalacion) * 0.16m;
                decimal totalFinal = totalCalculado + request.CostoInstalacion + ivaCalculado;

                // Actualizar la cotización principal
                cotizacion.IdProspecto = request.IdProspecto;
                cotizacion.CostoInstalacion = request.CostoInstalacion;
                cotizacion.Iva = ivaCalculado;
                cotizacion.TotalCotizado = totalFinal;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Cotización actualizada con éxito.", cotizacion });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Ocurrió un error al actualizar la cotización.", detalle = ex.Message });
            }
        }
    }

    // ==========================================
    // DTOs (Data Transfer Objects)
    // ==========================================

    public class AceptarCotizacionDto
    {
        public string MetodoPago { get; set; } = "Transferencia";
        public string? Descripcion { get; set; }
    }
}