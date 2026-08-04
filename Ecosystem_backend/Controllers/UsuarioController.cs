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

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        //                 EMPLEADOS
        // ==========================================

        [HttpPost("registro-empleado")]
        public async Task<IActionResult> RegistrarEmpleado([FromBody] RegistroEmpleadoDto request)
        {
            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo);
            if (existeUsuario) return BadRequest("El correo ya está registrado.");

            // 2. Crear el Usuario base con la contraseña encriptada
            var nuevoUsuario = new Usuario
            {
                Correo = request.Correo,
                // ¡Aquí ocurre la encriptación!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IdRol = 2,
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            var nuevoEmpleado = new Empleado
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                NombreCompleto = request.NombreCompleto,
                Puesto = request.Puesto,
                FechaIngreso = DateTime.Now
            };

            _context.Empleados.Add(nuevoEmpleado);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Empleado registrado con éxito", IdEmpleado = nuevoEmpleado.IdEmpleado });
        }

        [HttpGet("listar-empleados")]
        public async Task<IActionResult> ListarEmpleados()
        {
            var listaEmpleados = await _context.Empleados.ToListAsync();
            return Ok(listaEmpleados);
        }

        [HttpGet("empleado/{id}")]
        public async Task<IActionResult> ObtenerEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound("Empleado no encontrado.");
            return Ok(empleado);
        }

        [HttpPut("empleado/{id}")]
        public async Task<IActionResult> ActualizarEmpleado(int id, [FromBody] ActualizarEmpleadoDto request)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound("Empleado no encontrado.");

            empleado.NombreCompleto = request.NombreCompleto;
            empleado.Puesto = request.Puesto;

            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Empleado actualizado correctamente" });
        }

        [HttpDelete("empleado/{id}")]
        public async Task<IActionResult> EliminarEmpleado(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null) return NotFound("Empleado no encontrado.");

            var usuario = await _context.Usuarios.FindAsync(empleado.IdUsuario);
            if (usuario != null)
            {
                usuario.Activo = false; // Baja lógica: revoca el acceso al login
            }

            _context.Empleados.Remove(empleado);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Empleado eliminado y acceso al sistema revocado." });
        }

        // ==========================================
        //                 CLIENTES
        // ==========================================

        [HttpGet("listar-clientes")]
        public async Task<IActionResult> ListarClientes()
        {
            var listaClientes = await _context.Clientes.ToListAsync();
            return Ok(listaClientes);
        }

        [HttpGet("cliente/{id}")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound("Cliente no encontrado.");
            return Ok(cliente);
        }

        [HttpPut("cliente/{id}")]
        public async Task<IActionResult> ActualizarCliente(int id, [FromBody] ActualizarClienteDto request)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound("Cliente no encontrado.");

            cliente.Nombre = request.Nombre;
            cliente.Apellido = request.Apellido;
            cliente.Telefono = request.Telefono;
            cliente.Localidad = request.Localidad;
            cliente.Corporativo = request.Corporativo;

            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Cliente actualizado correctamente" });
        }

        [HttpDelete("cliente/{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound("Cliente no encontrado.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Cliente eliminado permanentemente." });
        }
    }
}