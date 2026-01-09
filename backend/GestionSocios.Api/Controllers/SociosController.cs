using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SociosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SociosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/Socios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Socio>>> Get()
        {
            var socios = await _db.Socios
                .AsNoTracking()
                .OrderBy(s => s.Apellido)
                .ThenBy(s => s.Nombre)
                .ToListAsync();

            return Ok(socios);
        }

        // GET: api/Socios/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Socio>> Get(int id)
        {
            var socio = await _db.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (socio == null) return NotFound();
            return Ok(socio);
        }

        // POST: api/Socios
        [HttpPost]
        public async Task<ActionResult<Socio>> Post([FromBody] CrearSocioDto dto)
        {
            // DNI único (además del índice en DB)
            var existeDni = await _db.Socios.AnyAsync(s => s.DNI == dto.DNI);
            if (existeDni) return BadRequest("Ya existe un socio con ese DNI.");

            var socio = new Socio
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                DNI = dto.DNI.Trim(),
                FechaNacimiento = dto.FechaNacimiento,
                Domicilio = dto.Domicilio?.Trim(),
                Actividad = dto.Actividad?.Trim(),
                Sexo = dto.Sexo,
                Activo = dto.Activo,
                FechaAlta = DateTime.UtcNow
            };

            _db.Socios.Add(socio);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = socio.Id }, socio);
        }

        // PUT: api/Socios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Socio socioActualizado)
        {
            if (id != socioActualizado.Id) return BadRequest("El ID no coincide");

            // Evitar que te “pisen” cosas no deseadas: adjuntamos y marcamos modificado
            _db.Entry(socioActualizado).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var existe = await _db.Socios.AnyAsync(s => s.Id == id);
                if (!existe) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Socios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var socio = await _db.Socios.FindAsync(id);
            if (socio == null) return NotFound();

            _db.Socios.Remove(socio);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
