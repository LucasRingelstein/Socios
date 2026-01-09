using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _config = config;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // 1) Verificamos si ya existe por email
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null) return BadRequest("El email ya está registrado.");

            // 2) Creamos el usuario Identity
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = $"{request.Nombre} {request.Apellido}".Trim()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // 3) (Opcional) Crear o vincular Socio por DNI
            //    Si tu idea es que todo usuario registrado sea socio, esto es útil.
            //    Si no, podés comentar este bloque y manejar socios por SociosController.
            if (!string.IsNullOrWhiteSpace(request.DNI))
            {
                var socio = await _db.Socios.FirstOrDefaultAsync(s => s.DNI == request.DNI);

                if (socio == null)
                {
                    socio = new Socio
                    {
                        Nombre = request.Nombre?.Trim() ?? "",
                        Apellido = request.Apellido?.Trim() ?? "",
                        DNI = request.DNI.Trim(),
                        Activo = true,
                        FechaAlta = DateTime.UtcNow,
                        UserId = user.Id
                    };

                    _db.Socios.Add(socio);
                }
                else
                {
                    // Si existe socio con ese DNI, lo vinculamos al usuario recién creado
                    socio.UserId = user.Id;

                    // opcional: actualizar nombre/apellido si están vacíos
                    if (string.IsNullOrWhiteSpace(socio.Nombre)) socio.Nombre = request.Nombre?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(socio.Apellido)) socio.Apellido = request.Apellido?.Trim() ?? "";
                }

                await _db.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Registrado con éxito" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return BadRequest("Usuario no encontrado");

            var ok = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!ok) return BadRequest("Contraseña incorrecta");

            // buscamos Socio vinculado (si existe)
            var socio = await _db.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == user.Id);

            return Ok(new { token = CrearToken(user, socio) });
        }

        private string CrearToken(ApplicationUser user, Socio? socio)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                // roles reales los agregamos después; por ahora ejemplo:
                new Claim(ClaimTypes.Role, "Admin")
            };

            if (socio != null)
            {
                claims.Add(new Claim("SocioId", socio.Id.ToString()));
            }

            var keyString = _config.GetSection("Jwt:Key").Value;
            if (string.IsNullOrWhiteSpace(keyString))
                throw new InvalidOperationException("Falta configurar Jwt:Key en appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
