using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SociosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SociosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. GET: api/Socios (Traer TODOS transformados a DTO)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SocioDto>>> Get()
        {
            // Usamos _db (que es tu variable correcta)
            var socios = await _db.Socios.AsNoTracking()
                .Select(s => new SocioDto
                {
                    Id = s.Id,
                    Nombre = s.Nombre,
                    Apellido = s.Apellido,
                    DNI = s.DNI,
                    FechaDeNacimiento = s.FechaNacimiento,

                    // EL CÁLCULO DE EDAD CORREGIDO:
                    // Preguntamos si tiene valor (.HasValue). Si tiene, calculamos. Si no, ponemos 0.
                    Edad = s.FechaNacimiento.HasValue ? (DateTime.Now.Year - s.FechaNacimiento.Value.Year) : 0,

                    Domicilio = s.Domicilio,
                    Actividad = s.Actividad,
                    Sexo = s.Sexo,
                    Activo = s.Activo,
                    FechaAlta = s.FechaAlta
                })
                .ToListAsync();

            return Ok(socios);
        }

        // 2. GET: api/Socios/5 (Traer UNO por ID) - ¡Este faltaba!
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Socio>> Get(int id)
        {
            var socio = await _db.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

            if (socio == null) return NotFound();

            return Ok(socio);
        }

        // 3. POST: api/Socios (Crear)
        // POST: api/Socios
        [HttpPost]
        public async Task<ActionResult<Socio>> Post(CrearSocioDto dto)
        {
            // 1. Validamos si ya existe ese DNI en la base de datos
            // (Esto evita que explote SQL si tenés una restricción UNIQUE)
            var existeDni = await _db.Socios.AnyAsync(s => s.DNI == dto.DNI);
            if (existeDni)
            {
                return BadRequest($"El DNI {dto.DNI} ya está registrado en el sistema.");
            }

            // 2. MAPPING: Pasamos los datos del "Papelito" (DTO) a la "Carpeta Real" (Entidad)
            var nuevoSocio = new Socio
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                DNI = dto.DNI,
                // Si viene fecha nula, ponemos una por defecto o la dejamos null (según tu base de datos)
                FechaNacimiento = dto.FechaNacimiento ?? DateTime.MinValue,
                Domicilio = dto.Domicilio,
                Actividad = dto.Actividad,
                Sexo = dto.Sexo,
                Activo = dto.Activo,

                // 3. CAMPOS AUTOMÁTICOS (El usuario no los toca)
                FechaAlta = DateTime.UtcNow, // Ponemos la fecha de hoy
                UserId = null // Por ahora null, después veremos si lo atamos al usuario logueado
            };

            // 4. Guardamos en la Base de Datos
            _db.Socios.Add(nuevoSocio);
            await _db.SaveChangesAsync();

            // 5. Retornamos el 201 Created con el objeto creado
            return CreatedAtAction(nameof(Get), new { id = nuevoSocio.Id }, nuevoSocio);
        }

        // 4. PUT: api/Socios/5 (Editar usando DTO)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, CrearSocioDto dto)
        {
            // 1. Buscamos el socio en la base de datos
            var socioExistente = await _db.Socios.FindAsync(id);

            if (socioExistente == null) return NotFound($"No se encontró el socio con ID {id}");

            // 2. Validación de DNI:
            // Si el DNI que mandan es distinto al que ya tenía, verificamos que no lo esté usando OTRO socio.
            if (dto.DNI != socioExistente.DNI)
            {
                var dniOcupado = await _db.Socios.AnyAsync(s => s.DNI == dto.DNI && s.Id != id);
                if (dniOcupado)
                {
                    return BadRequest("Ese DNI ya pertenece a otro socio.");
                }
            }

            // 3. MAPPING: Actualizamos SOLO los campos permitidos
            // (El ID, FechaAlta y UserId NO se tocan, se quedan como estaban)
            socioExistente.Nombre = dto.Nombre;
            socioExistente.Apellido = dto.Apellido;
            socioExistente.DNI = dto.DNI;
            socioExistente.FechaNacimiento = dto.FechaNacimiento ?? DateTime.MinValue;
            socioExistente.Domicilio = dto.Domicilio;
            socioExistente.Actividad = dto.Actividad;
            socioExistente.Sexo = dto.Sexo;
            socioExistente.Activo = dto.Activo;

            // 4. Guardamos los cambios
            await _db.SaveChangesAsync();

            return NoContent(); // 204: Todo salió bien y no devuelvo nada
        }

        // 5. DELETE: api/Socios/5 (Borrar)
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