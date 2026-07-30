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
    public class GastosController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos el contexto de la base de datos
        public GastosController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Registrar un nuevo gasto (Sistema de Administración - Gastos)
        // http://localhost:5048/api/Gastos/registrar-gasto
        /*
         * POST
         {
           "Fecha": "2024-06-01T00:00:00",
           "IdProveedor": 1,
           "Concepto": "Compra de insumos",
           "Total": 1500.75
         }
         */
        [HttpPost("registrar-gasto")]
        public async Task<IActionResult> RegistrarGasto([FromBody] RegistroGastoDto request)
        {
            try
            {
                // Validamos que el proveedor exista si se proporciona un IdProveedor
                var proveedor = await _context.Proveedores.FindAsync(request.IdProveedor);
                if (proveedor == null)
                {
                    return BadRequest(new { Mensaje = "El proveedor no existe." });
                }

                // Creamos el nuevo gasto
                var nuevoGasto = new Gasto
                {
                    Fecha = request.Fecha,
                    IdProveedor = request.IdProveedor,
                    Concepto = request.Concepto,
                    Total = request.Total
                };
                _context.Gastos.Add(nuevoGasto);
                await _context.SaveChangesAsync();
                return Ok(new { Mensaje = "Gasto registrado con éxito", gasto = nuevoGasto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error Interno del servidor");
            }
        }

        // 2. Obtener todos los gastos (Sistema de Administración - Gastos)
        // http://localhost:5048/api/Gastos/listar-gastos
        /*
         * GET
         */
        [HttpGet("listar-gastos")]
        public async Task<IActionResult> ListarGastos()
        {
            try
            {
                // Obtenemos todos los gastos con sus proveedores asociados
                var listaGastos = await _context.Gastos.Include(g => g.Proveedor).ToListAsync();

                return Ok(listaGastos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensaje = "Ocurrió un error al obtener los gastos.", Error = ex.Message });
            }
        }

        // 3. Actualizar un gasto existente (Sistema de Administración - Gastos)
        // http://localhost:5048/api/Gastos/actualizar-gasto/{id}
        /* PUT
         {
           "Fecha": "2024-06-01T00:00:00",
           "IdProveedor": 1,
           "Concepto": "Compra de insumos actualizada",
           "Total": 1600.00
         }
         */
        [HttpPut("actualizar-gasto/{id}")]
        public async Task<IActionResult> ActualizarGasto(int id, [FromBody] ActualizarGastoDto request)
        {
            try
            {
                var gastoExistente = await _context.Gastos.FindAsync(id);
                if (gastoExistente == null)
                {
                    return NotFound(new { Mensaje = "El gasto no existe." });
                }
                // Validamos que el proveedor exista si se proporciona un IdProveedor
                if (request.IdProveedor.HasValue)
                {
                    var proveedor = await _context.Proveedores.FindAsync(request.IdProveedor.Value);
                    if (proveedor == null)
                    {
                        return BadRequest(new { Mensaje = "El proveedor no existe." });
                    }
                }

                // Actualizamos los campos del gasto existente
                gastoExistente.Fecha = request.Fecha;
                gastoExistente.IdProveedor = request.IdProveedor;
                gastoExistente.Concepto = request.Concepto;
                gastoExistente.Total = request.Total;
                _context.Gastos.Update(gastoExistente);
                await _context.SaveChangesAsync();
                return Ok(new { Mensaje = "Gasto actualizado con éxito", gasto = gastoExistente });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensaje = "Ocurrió un error al actualizar el gasto.", Error = ex.Message });
            }
        }

        // 4. Eliminar un gasto existente (Sistema de Administración - Gastos)
        // http://localhost:5048/api/Gastos/eliminar-gasto/{id}
        /* 
         * DELETE
         */
        [HttpDelete("eliminar-gasto/{id}")]
        public async Task<IActionResult> EliminarGasto(int id)
        {
            try
            {
                // Buscamos el gasto existente por su Id
                var gastoExistente = await _context.Gastos.FindAsync(id);
                if (gastoExistente == null)
                {
                    return NotFound(new { Mensaje = "El gasto no existe." });
                }

                _context.Gastos.Remove(gastoExistente);
                await _context.SaveChangesAsync();
                return Ok(new { Mensaje = "Gasto eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensaje = "Ocurrió un error al eliminar el gasto.", Error = ex.Message });
            }
        }
    }
}

