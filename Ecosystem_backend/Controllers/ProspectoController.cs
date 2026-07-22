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
    public class ProspectoController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos el contexto de la base de datos
        public ProspectoController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Registrar un nuevo prospecto (Landing Page - Público)
        // http://localhost:5048/api/Prospecto/registrar-prospecto
        /*
         * POST
         {
          "Nombre": "Uriel",
          "Apellido": "Hernandez",
          "Telefono": "47712345600",
          "Corporativo": "",
          "Localidad": "León"
        }
         */
        [HttpPost("registrar-prospecto")]
        public async Task<IActionResult> RegistrarProspecto([FromBody] RegistroProspectoDto request)
        {
            // Validamos que el prospecto no exista ya (por teléfono)
            var existeProspecto = await _context.Prospectos.AnyAsync(p => p.Telefono == request.Telefono);
            if (existeProspecto)
            {
                return BadRequest(new { Mensaje = "El prospecto ya está registrado." });
            }
            // Creamos el nuevo prospecto
            var nuevoProspecto = new Prospecto
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Telefono = request.Telefono,
                Corporativo = request.Corporativo,
                Localidad = request.Localidad,
                Estatus = "Pendiente"
            };

            _context.Prospectos.Add(nuevoProspecto);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Prospecto registrado con éxito", prospecto = nuevoProspecto });
        }

        // 2. Seguimiento de prospectos pendientes (Sistema de Administración - Prospectos)
        // http://localhost:5048/api/Prospecto/listar-prospectos
        /*
         * GET
         */
        [HttpGet("listar-prospectos")]
        public async Task<IActionResult> ListarProspectos()
        {
            // Usamos .Where() para filtrar la base de datos antes de traer los resultados
            var listaProspectos = await _context.Prospectos
                                                .Where(p => p.Estatus == "Pendiente")
                                                .ToListAsync();
            return Ok(listaProspectos);
        }

        // 3. Seguimiento de prospectos por estado (Sistema de Administración - Prospectos)
        // http://localhost:5048/api/Prospecto/listar-prospectos/Aceptado

        /*
         * GET
         * Opciones: Pendiente, Contactado, Aceptado, Cancelado
         */
        [HttpGet("listar-prospectos/{estado}")]
        public async Task<IActionResult> ListarProspectosPorEstado(string estado)
        {
            // Filtramos dinámicamente usando la variable 'estado' que viene en la URL
            var listaProspectos = await _context.Prospectos
                                                .Where(p => p.Estatus == estado)
                                                .ToListAsync();

            // Opcional: Un mensajito si la lista viene vacía
            if (!listaProspectos.Any())
            {
                return Ok(new { Mensaje = $"No se encontraron prospectos con el estatus '{estado}'.", Datos = listaProspectos });
            }

            return Ok(listaProspectos);
        }

    }
}