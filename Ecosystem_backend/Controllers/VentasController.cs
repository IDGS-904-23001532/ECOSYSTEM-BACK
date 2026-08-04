using Ecosystem_backend.Data;
using Ecosystem_backend.DTOs;
using Ecosystem_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VentasController> _logger;

        public VentasController(AppDbContext context, ILogger<VentasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/<VentasController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var ventas = await _context.Ventas.Include(v => v.Cliente).ToListAsync();
                return Ok(ventas);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener las ventas");
                return BadRequest("Ocurrió un error interno, comunícate al servicio técnico");
            }
        }

        // GET api/<VentasController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var venta = await _context.Ventas.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.IdVenta == id);

                if (venta == null)
                {
                    return NotFound("No se encontró la venta");
                }

                return Ok(venta);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener la venta con id {Id}", id);
                return BadRequest("Ocurrió un error interno, comunícate al servicio técnico");
            }
        }

        // POST api/<VentasController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VentaDTO ventaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Datos inválidos");
                }

                // Validamos que el cliente exista
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == ventaDto.IdCliente);
                if (!clienteExiste)
                {
                    return BadRequest($"No existe un cliente con id {ventaDto.IdCliente}");
                }

                // Validamos MetodoPago y Estado
                if (ventaDto.MetodoPago != "Efectivo" && ventaDto.MetodoPago != "Transferencia")
                {
                    return BadRequest("MetodoPago debe ser 'Efectivo' o 'Transferencia'");
                }

                if (ventaDto.Estado != "Pendiente" && ventaDto.Estado != "Completo")
                {
                    return BadRequest("Estado debe ser 'Pendiente' o 'Completo'");
                }

                var venta = new Venta
                {
                    IdCliente = ventaDto.IdCliente,
                    Total = ventaDto.Total,
                    Descripcion = ventaDto.Descripcion,
                    MetodoPago = ventaDto.MetodoPago,
                    Estado = ventaDto.Estado
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                return Ok("Venta creada correctamente");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al crear la venta");
                return BadRequest("Ocurrió un error interno, comunícate al servicio técnico");
            }
        }

        // PUT api/<VentasController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] VentaDTO ventaDto)
        {
            try
            {
                var ventaExistente = await _context.Ventas.FindAsync(id);

                if (ventaExistente == null)
                {
                    return NotFound("No se encontraron coincidencias");
                }

                var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == ventaDto.IdCliente);
                if (!clienteExiste)
                {
                    return BadRequest($"No existe un cliente con id {ventaDto.IdCliente}");
                }

                if (ventaDto.MetodoPago != "Efectivo" && ventaDto.MetodoPago != "Transferencia")
                {
                    return BadRequest("MetodoPago debe ser 'Efectivo' o 'Transferencia'");
                }

                if (ventaDto.Estado != "Pendiente" && ventaDto.Estado != "Completo")
                {
                    return BadRequest("Estado debe ser 'Pendiente' o 'Completo'");
                }

                ventaExistente.IdCliente = ventaDto.IdCliente;
                ventaExistente.Total = ventaDto.Total;
                ventaExistente.Descripcion = ventaDto.Descripcion;
                ventaExistente.MetodoPago = ventaDto.MetodoPago;
                ventaExistente.Estado = ventaDto.Estado;

                await _context.SaveChangesAsync();

                return Ok("Se actualizó correctamente");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al actualizar la venta con id {Id}", id);
                return BadRequest("Ocurrió un error interno, comunícate al servicio técnico");
            }
        }

        // DELETE api/<VentasController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var venta = await _context.Ventas.FindAsync(id);

                if (venta == null)
                {
                    return NotFound("No se encontró coincidencias, revisa de nuevo");
                }

                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();

                return Ok("Venta eliminada correctamente");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al eliminar la venta con id {Id}", id);
                return BadRequest("Ocurrió un error interno, comunícate al servicio técnico");
            }
        }
    }
}