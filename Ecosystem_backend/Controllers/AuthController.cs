using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecosystem_backend.Data;
using Ecosystem_backend.Models;
using Ecosystem_backend.DTOs;

namespace Ecosystem_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // 1. Validar que el usuario exista
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (usuario == null || !usuario.Activo)
                return Unauthorized(new { Mensaje = "Credenciales incorrectas o cuenta inactiva." });

            // 2. Validar la contraseña encriptada con BCrypt
            bool passwordValido = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            if (!passwordValido)
                return Unauthorized(new { Mensaje = "Credenciales incorrectas." });

            // 3. Generar el Token JWT
            var jwtKey = _config["Jwt:Key"];
            var keyBytes = Encoding.ASCII.GetBytes(jwtKey!);
            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(8), // El token dura 8 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(tokenConfig);

            // 4. Guardar la sesión en la tabla temporal SQL
            var nuevaSesion = new SesionTemporal
            {
                IdUsuario = usuario.IdUsuario,
                TokenJWT = tokenString,
                FechaInicio = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddHours(8)
            };

            _context.SesionesTemporales.Add(nuevaSesion);
            await _context.SaveChangesAsync();

            // 5. Devolver la respuesta al frontend
            return Ok(new
            {
                Mensaje = "Login exitoso",
                Token = tokenString,
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return BadRequest("Token inválido.");

            var token = authHeader.Substring("Bearer ".Length);

            // Buscamos la sesión temporal y la eliminamos (cierre de sesión real)
            var sesionActiva = await _context.SesionesTemporales.FirstOrDefaultAsync(s => s.TokenJWT == token);

            if (sesionActiva != null)
            {
                _context.SesionesTemporales.Remove(sesionActiva);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Mensaje = "Sesión cerrada correctamente." });
        }
    }
}