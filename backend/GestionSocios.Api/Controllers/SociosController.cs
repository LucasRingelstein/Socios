using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs; // Asumo que existe este namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Comenta esto temporalmente si necesitas probar sin login
    public class SociosController : ControllerBase
    {
        private readonly UserManager<Socio> _userManager;
        private readonly ApplicationDbContext _db;

        // CORRECCIÓN 1: Inyectar correctamente el UserManager en el constructor
        public SociosController(ApplicationDbContext db, UserManager<Socio> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: api/Socios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Socio>>> Get() // Cambié SocioDto a Socio para simplificar, ajusta si usas DTO
        {
            // Usamos _userManager aquí porque ya está optimizado para usuarios
            var socios = await _userManager.Users
                .AsNoTracking()
                .OrderBy(s => s.Apellido)
                .ThenBy(s => s.Nombre)
                .ToListAsync();

            return Ok(socios);
        }

        // GET api/Socios/{id_string}
        // CORRECCIÓN 2: El id debe ser string porque IdentityUser usa strings (GUIDs)
        [HttpGet("{id}")]
        public async Task<ActionResult<Socio>> Get(string id)
        {
            // IdentityUser usa Id como string
            var socio = await _db.Socios.FirstOrDefaultAsync(s => s.Id == id);

            if (socio == null) return NotFound();

            return Ok(socio);
        }

        // POST api/Socios
        [HttpPost]
        public async Task<ActionResult<Socio>> Post([FromBody] Socio nuevoSocio)
        {
            // CORRECCIÓN 3: Al usar Identity, lo ideal es usar _userManager.CreateAsync
            // para que hashee contraseñas y valide duplicados, pero si lo haces directo a la BD:

            // Generar ID si viene vacío (Identity suele requerir GUIDs)
            if (string.IsNullOrEmpty(nuevoSocio.Id))
            {
                nuevoSocio.Id = Guid.NewGuid().ToString();
            }

            // Normalizar campos que Identity requiere (Email, UserName, etc.)
            nuevoSocio.UserName = nuevoSocio.Nombre + nuevoSocio.Apellido; // Ejemplo simple
            nuevoSocio.NormalizedUserName = nuevoSocio.UserName.ToUpper();

            _db.Socios.Add(nuevoSocio);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = nuevoSocio.Id }, nuevoSocio);
        }

        // PUT api/Socios/{id_string}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] Socio socioActualizado)
        {
            // Comparar strings
            if (id != socioActualizado.Id) return BadRequest("El ID no coincide");

            _db.Entry(socioActualizado).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Socios.Any(s => s.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE api/Socios/{id_string}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // FindAsync funciona bien con la clave primaria correcta
            var socio = await _db.Socios.FindAsync(id);

            if (socio == null) return NotFound();

            _db.Socios.Remove(socio);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}