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
    public class CotizacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos el contexto de la base de datos
        public CotizacionController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Crear cotización (Sistema Gestion - Prospectos)
        // http://localhost:5048/api/Cotizacion/registrar-cotizacion
        /*
         * POST
         {
          "IdProspecto": 1,
          "Detalles": [
            {
              "IdProducto": 5,
              "Cantidad": 2
            },
            {
              "IdProducto": 8,
              "Cantidad": 1
            }
          ]
        }
         */
        [HttpPost("registrar-cotizacion")]
        public async Task<IActionResult> RegistrarCotizacion([FromBody] RegistroCotizacionDto request)
        {
            // 1. Validaciones iniciales
            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest(new { Mensaje = "La cotización debe incluir al menos un producto." });

            var prospecto = await _context.Prospectos.FindAsync(request.IdProspecto);
            if (prospecto == null)
                return NotFound(new { Mensaje = "El prospecto seleccionado no existe." });

            // 2. Iniciar una transacción para proteger la integridad de los datos
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3. Crear la cabecera de la cotización
                var nuevaCotizacion = new Cotizacion
                {
                    IdProspecto = request.IdProspecto,
                    FechaEmision = DateTime.Now,
                    Estatus = "Pendiente",
                    TotalCotizado = 0 // Se calculará después de agregar los detalles
                };

                _context.Cotizaciones.Add(nuevaCotizacion);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id de la cotización

                decimal totalCalculado = 0; // Variable para acumular el total de la cotización

                // 4. Agregar los detalles de la cotización y calcular el total
                foreach (var item in request.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.IdProducto);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { Mensaje = $"El producto con ID {item.IdProducto} no existe." });
                    }

                    // Calcular el subtotal por producto
                    decimal subtotalItem = producto.Precio * item.Cantidad;
                    totalCalculado += subtotalItem; // Acumulamos el subtotal al total de la cotización

                    var nuevoDetalle = new DetalleCotizacion
                    {
                        IdCotizacion = nuevaCotizacion.IdCotizacion,
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad, // <-- AQUÍ FALTABA LA COMA
                        Subtotal = subtotalItem
                    };

                    _context.DetallesCotizaciones.Add(nuevoDetalle);
                }

                // 5. Actualizar el total cotizado en la cabecera de la cotización
                nuevaCotizacion.TotalCotizado = totalCalculado;

                // 6. Cambiar el estatus del prospecto a "Contactado"
                if (prospecto.Estatus == "Pendiente")
                {
                    prospecto.Estatus = "Contactado";
                }

                // 7. Guardar los cambios y confirmar la transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Mensaje = "Cotización registrada con éxito", IdCotizacion = nuevaCotizacion.IdCotizacion, Total = totalCalculado });
            }
            catch (Exception ex)
            {
                // En caso de error, revertimos la transacción y devolvemos un mensaje de error
                await transaction.RollbackAsync();
                return BadRequest(new { Mensaje = "Error al registrar la cotización", Detalle = ex.Message });
            }
        }

        // 2. Ver cotizaciones (Sistema Gestion - Prospectos)
        // http://localhost:5048/api/Cotizacion/listar-cotizaciones
        /*
         * GET
         */
        [HttpGet("listar-cotizaciones")]
        public async Task<IActionResult> ListarCotizaciones()
        {
            // Usamos .Where() para filtrar la base de datos antes de traer los resultados
            var listaCotizaciones = await _context.Cotizaciones
                                                .Where(c => c.Estatus == "Pendiente")
                                                .ToListAsync();
            return Ok(listaCotizaciones);
        }

        // 3. Ver cotizaciones por prospecto (Sistema Gestion - Prospectos)
        // http://localhost:5048/api/Cotizacion/ver-completa/1
        /*
         * GET
         */
        [HttpGet("ver-completa/{idCotizacion}")]
        public async Task<IActionResult> VerCotizacionCompleta(int idCotizacion)
        {
            // Buscamos la cotización incluyendo los datos generales del prospecto
            var cotizacion = await _context.Cotizaciones
                .Include(c => c.Prospecto)
                .FirstOrDefaultAsync(c => c.IdCotizacion == idCotizacion);

            if (cotizacion == null)
                return NotFound(new { Mensaje = "La cotización solicitada no existe." });

            // Buscamos los detalles (productos y cantidades) vinculados a esta cotización
            var detalles = await _context.DetallesCotizaciones
                .Where(d => d.IdCotizacion == idCotizacion)
                .Include(d => d.Producto) // Incluye los datos del catálogo de productos si es necesario
                .ToListAsync();

            // Retornamos un objeto estructurado con el encabezado y el detalle
            return Ok(new
            {
                Encabezado = cotizacion,
                Detalles = detalles
            });
        }

        // 4. Captación de cotizaciones (Landing Page - Público)
        // http://localhost:5048/api/Cotizacion/cerrar-cotizacion/1
        /*
         * PUT
            {
            "Accion": "aceptar",
            "Correo": "usuario.gmail.com",
            "Password": "123456"
            }
         */
        [HttpPut("cerrar-cotizacion/{idCotizacion}")]
public async Task<IActionResult> CerrarCotizacion(int idCotizacion, [FromBody] CierreCotizacionDto request)
{
    var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion);
    if (cotizacion == null)
        return NotFound(new { Mensaje = "Cotización no encontrada." });

    var prospecto = await _context.Prospectos.FindAsync(cotizacion.IdProspecto);
    if (prospecto == null)
        return NotFound(new { Mensaje = "Prospecto asociado no encontrado." });

    if (cotizacion.Estatus != "Pendiente")
        return BadRequest(new { Mensaje = "Esta cotización ya fue cerrada previamente." });

    // Usamos una transacción para asegurar que todo pase o nada se guarde si ocurre un error
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        string accion = request.Accion.ToLower();

        if (accion == "aceptar")
        {
            // 1. Actualizar estatus de los modelos principales
            cotizacion.Estatus = "Aceptada";
            prospecto.Estatus = "Aceptado";


            // 2. Crear un nuevo usuario en la tabla Usuarios con el rol de 'Clientes'
            var nuevoUsuario = new Usuario
            {
                Correo = request.Correo,
                PasswordHash = request.Password, // <-- CORREGIDO DE request.Password A request.Contrasena
                IdRol = 1, //  1 es para 'Clientes'
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // 3. Clonar datos del prospecto a la tabla Clientes y asignar credenciales y rol
            var nuevoCliente = new Cliente
            {
                Nombre = prospecto.Nombre,
                Apellido = prospecto.Apellido,
                Telefono = prospecto.Telefono,
                Corporativo = prospecto.Corporativo,
                Localidad = prospecto.Localidad,
                FechaRegistro = DateTime.Now
            };

            _context.Clientes.Add(nuevoCliente);
        }
        else if (accion == "rechazar")
        {
            // 1. Cambiar estados en caso de declinar
            cotizacion.Estatus = "Rechazada";
            prospecto.Estatus = "Cancelado"; // Baja lógica del interesado
        }
        else
        {
            return BadRequest(new { Mensaje = "Acción no válida. Utilice 'aceptar' o 'rechazar'." });
        }

        // Guardar cambios y confirmar la transacción en SQL Server
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { Mensaje = $"La cotización ha sido procesada como {request.Accion.ToUpper()} exitosamente." });
    }
    catch (Exception ex)
    {
        // Si algo falla, se deshacen todos los cambios parciales
        await transaction.RollbackAsync();
        return StatusCode(500, new { Mensaje = "Error interno al procesar el cierre.", Detalle = ex.Message });
    }
}

        // 5. Listar cotizaciones por usuario (Sistema Gestion - Clientes)
        // http://localhost:5048/api/Cotizacion/usuario/1
        /*
         * GET
         */
        [HttpGet("usuario/{idUsuario}")]
public async Task<IActionResult> ListarCotizacionesPorUsuario(int idUsuario)
{
    // 1. Validar que el usuario exista
    var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
    if (!usuarioExiste)
    {
        return NotFound(new { Mensaje = "El usuario especificado no existe." });
    }

    // 2. Buscamos el perfil de cliente asociado a ese IdUsuario
    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);
    if (cliente == null)
    {
        return Ok(new List<Cotizacion>()); // Retorna lista vacía si el usuario no tiene perfil de cliente
    }

    // 3. Consultar las cotizaciones
    // Nota: Como la cotización se vincula originalmente al IdProspecto, filtramos las cotizaciones 
    // cuyos datos de prospecto coincidan con el cliente (o por IdProspecto si tu tabla Clientes guarda esa llave).
    var cotizaciones = await _context.Cotizaciones
        .Include(c => c.Prospecto)
        .Where(c => c.Prospecto.Telefono == cliente.Telefono) // O cl.IdProspecto == c.IdProspecto si agregaron esa FK
        .OrderByDescending(c => c.FechaEmision)
        .ToListAsync();

    return Ok(cotizaciones);
}

        // 6. Editar una cotización existente
        // http://localhost:5048/api/Cotizacion/editar-cotizacion/1
        /*
         * PUT
            {
              "IdProspecto": 1,
              "Detalles": [
                {
                  "IdProducto": 5,
                  "Cantidad": 3
                },
                {
                  "IdProducto": 8,
                  "Cantidad": 2
                }
              ]
            }
         */
        [HttpPut("editar-cotizacion/{idCotizacion}")]
        public async Task<IActionResult> EditarCotizacion(int idCotizacion, [FromBody] RegistroCotizacionDto request)
        {
            if (request.Detalles == null || !request.Detalles.Any())
                return BadRequest(new { Mensaje = "La cotización debe incluir al menos un producto." });

            var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion);
            if (cotizacion == null)
                return NotFound(new { Mensaje = "Cotización no encontrada." });

            // Validación de negocio: Solo editar si está pendiente
            if (cotizacion.Estatus != "Pendiente")
                return BadRequest(new { Mensaje = "Solo se pueden modificar cotizaciones en estatus 'Pendiente'." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Borrar los detalles anteriores
                var detallesViejos = await _context.DetallesCotizaciones
                    .Where(d => d.IdCotizacion == idCotizacion)
                    .ToListAsync();

                _context.DetallesCotizaciones.RemoveRange(detallesViejos);

                decimal totalCalculado = 0;

                // 2. Insertar los nuevos detalles y recalcular
                foreach (var item in request.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.IdProducto);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { Mensaje = $"El producto con ID {item.IdProducto} no existe." });
                    }

                    decimal subtotalItem = producto.Precio * item.Cantidad;
                    totalCalculado += subtotalItem;

                    var nuevoDetalle = new DetalleCotizacion
                    {
                        IdCotizacion = cotizacion.IdCotizacion,
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        Subtotal = subtotalItem
                    };

                    _context.DetallesCotizaciones.Add(nuevoDetalle);
                }

                // 3. Actualizar la cabecera
                cotizacion.IdProspecto = request.IdProspecto;
                cotizacion.TotalCotizado = totalCalculado;
                cotizacion.FechaEmision = DateTime.Now; // Se actualiza la fecha a la última modificación

                // 4. Guardar cambios
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Mensaje = "Cotización actualizada con éxito", Total = totalCalculado });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Mensaje = "Error al actualizar la cotización.", Detalle = ex.Message });
            }
        }

        // 6. Eliminar una cotización
        // http://localhost:5048/api/Cotizacion/eliminar-cotizacion/1
        /*
         * DELETE
         */
        [HttpDelete("eliminar-cotizacion/{idCotizacion}")]
        public async Task<IActionResult> EliminarCotizacion(int idCotizacion)
        {
            var cotizacion = await _context.Cotizaciones.FindAsync(idCotizacion);
            if (cotizacion == null)
                return NotFound(new { Mensaje = "Cotización no encontrada." });

            // Validación de negocio: Evitar eliminar historial cerrado
            if (cotizacion.Estatus != "Pendiente")
                return BadRequest(new { Mensaje = "No se puede eliminar una cotización que ya fue aceptada o rechazada." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Eliminar primero los detalles (los productos cotizados)
                var detalles = await _context.DetallesCotizaciones
                    .Where(d => d.IdCotizacion == idCotizacion)
                    .ToListAsync();

                _context.DetallesCotizaciones.RemoveRange(detalles);

                // 2. Eliminar la cabecera de la cotización
                _context.Cotizaciones.Remove(cotizacion);

                // 3. Guardar cambios en la base de datos
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Mensaje = "Cotización eliminada correctamente del sistema." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Mensaje = "Error al eliminar la cotización.", Detalle = ex.Message });
            }
        }
    }
}