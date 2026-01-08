using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Socio> _userManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public AuthController(UserManager<Socio> userManager, IConfiguration config, ApplicationDbContext db)
        {
            _userManager = userManager;
            _config = config;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // Verificamos si ya existe por email
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null) return BadRequest("El email ya está registrado.");

            // Creamos la instancia de Socio (que hereda de IdentityUser)
            var socio = new Socio
            {
                UserName = request.Email,
                Email = request.Email,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                DNI = request.DNI,
                Activo = true,
                FechaAlta = DateTime.Now
                // No asignamos PasswordHash acá, lo hace el UserManager abajo
            };

            // El UserManager crea el usuario y hashea la password automáticamente
            var result = await _userManager.CreateAsync(socio, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { mensaje = "Registrado con éxito en AspNetUsers" });
            }

            // Si hay errores (ej: contraseña débil), Identity nos devuelve la lista acá
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            // Buscamos al socio por email
            var socio = await _userManager.FindByEmailAsync(request.Email);

            if (socio == null) return BadRequest("Usuario no encontrado");

            // Verificamos la password contra el hash de la DB
            var result = await _userManager.CheckPasswordAsync(socio, request.Password);

            if (!result)
            {
                return BadRequest("Contraseña incorrecta");
            }

            // Si el login es correcto, generamos el token
            return Ok(new { token = CrearToken(socio) });
        }

        private string CrearToken(Socio socio)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, socio.Email!),
                new Claim(ClaimTypes.NameIdentifier, socio.Id), // El ID ahora es string
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("SocioId", socio.Id)
            };

            var keyString = _config.GetSection("Jwt:Key").Value;
            if (string.IsNullOrEmpty(keyString)) keyString = "ClaveDeEmergencia_DebeSerMuyLarga_2026";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}