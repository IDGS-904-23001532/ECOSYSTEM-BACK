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

        [HttpPost("registro-cliente")]
        public async Task<IActionResult> RegistrarCliente([FromBody] RegistroClienteDto request)
        {
            // 1. Validar que el correo no exista ya
            var existeUsuario = _context.Usuarios.Any(u => u.Correo == request.Correo);
            if (existeUsuario)
            {
                return BadRequest("El correo ya está registrado.");
            }

            // 2. Crear el Usuario base
            var nuevoUsuario = new Usuario
            {
                Correo = request.Correo,
                // tenemos que encriptar la contrasenia (prox)
                PasswordHash = request.Password,
                IdRol = 1, //  es el ID del rol 'Cliente' en la BD
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync(); // Guardamos para que se genere el IdUsuario

            // 3. Crear el Cliente vinculado al Usuario recién creado
            var nuevoCliente = new Cliente
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                NombreCompleto = request.NombreCompleto,
                DireccionInstalacion = request.DireccionInstalacion,
                Telefono = request.Telefono
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync(); // Guardamos el cliente

            return Ok(new { Mensaje = "Cliente registrado con éxito", IdCliente = nuevoCliente.IdCliente });
        }

        [HttpPost("registro-empleado")]
        public async Task<IActionResult> RegistrarEmpleado([FromBody] RegistroEmpleadoDto request)
        {
            // 1. Validar que el correo no exista ya en el sistema
            var existeUsuario = _context.Usuarios.Any(u => u.Correo == request.Correo);
            if (existeUsuario)
            {
                return BadRequest("El correo ya está registrado.");
            }

            // 2. Crear el Usuario base
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
            
            var listaClientes = _context.Clientes.ToList();

            return Ok(listaClientes);
        }

        [HttpGet("listar-empleados")]
        public async Task<IActionResult> ListarEmpleados()
        {
            var listaEmpleados = _context.Empleados.ToList();

            return Ok(listaEmpleados);
        }
    }
}
