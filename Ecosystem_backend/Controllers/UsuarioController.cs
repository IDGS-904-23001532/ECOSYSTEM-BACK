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
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Inyectamos el contexto de la base de datos
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // ❌ Eliminamos RegistrarCliente porque ahora los clientes nacen al aceptar una cotización

        [HttpPost("registro-empleado")]
        public async Task<IActionResult> RegistrarEmpleado([FromBody] RegistroEmpleadoDto request)
        {
            // 1. Validar que el correo no exista ya en el sistema usando AnyAsync
            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo);
            if (existeUsuario)
            {
                return BadRequest("El correo ya está registrado.");
            }

            // 2. Crear el Usuario base (AHORA EXCLUSIVO PARA EMPLEADOS)
            var nuevoUsuario = new Usuario
            {
                Correo = request.Correo,
                // Aqui tambien tenemos q encriptar despues
                PasswordHash = request.Password,
                IdRol = 2, //  2 es para 'Empleado'
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync(); // Se genera el IdUsuario

            // 3. Crear el Empleado vinculado
            var nuevoEmpleado = new Empleado
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                NombreCompleto = request.NombreCompleto,
                Puesto = request.Puesto,
                FechaIngreso = DateTime.Now // Registramos el momento exacto
            };

            _context.Empleados.Add(nuevoEmpleado);
            await _context.SaveChangesAsync(); // Se guarda en la BD

            return Ok(new { Mensaje = "Empleado registrado con éxito", IdEmpleado = nuevoEmpleado.IdEmpleado });
        }

        [HttpGet("listar-clientes")]
        public async Task<IActionResult> ListarClientes()
        {
            // Usamos ToListAsync para no bloquear el hilo de ejecución
            var listaClientes = await _context.Clientes.ToListAsync();

            return Ok(listaClientes);
        }

        [HttpGet("listar-empleados")]
        public async Task<IActionResult> ListarEmpleados()
        {
            var listaEmpleados = await _context.Empleados.ToListAsync();

            return Ok(listaEmpleados);
        }
    }
}
