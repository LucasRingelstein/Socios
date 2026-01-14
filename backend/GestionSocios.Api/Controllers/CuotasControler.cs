using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using GestionSocios.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")] // ¡Protegido con llave! 🔐
    public class CuotasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CuotasController(ApplicationDbContext db)
        {
            _db = db;
        }

      

        // 2. POST: api/Cuotas (Generar una nueva deuda a un socio)
        [HttpPost]
        public async Task<ActionResult<Cuota>> Post(CrearCuotaDto dto)
        {
            // A. Validar que el socio exista
            var socio = await _db.Socios.FindAsync(dto.SocioId);
            if (socio == null)
            {
                return BadRequest($"El socio con ID {dto.SocioId} no existe.");
            }

            // B. Validar que no tenga YA una cuota para ese Mes y Año
            var existeCuota = await _db.Cuotas.AnyAsync(c => c.SocioId == dto.SocioId && c.Mes == dto.Mes && c.Anio == dto.Anio);
            if (existeCuota)
            {
                return BadRequest($"El socio {socio.Apellido} ya tiene generada la cuota de {dto.Mes}/{dto.Anio}.");
            }

            // C. Crear la Cuota
            var nuevaCuota = new Cuota
            {
                SocioId = dto.SocioId,
                Mes = dto.Mes,
                Anio = dto.Anio,
                Monto = dto.Monto,
                Estado = "Pendiente" // Por defecto nace debiendo plata
            };

            _db.Cuotas.Add(nuevaCuota);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Cuota generada con éxito", id = nuevaCuota.Id });
        }

        // 3. PUT: api/Cuotas/5/pagar (Registrar el pago)
        [HttpPut("{id:int}/pagar")]
        public async Task<IActionResult> Pagar(int id)
        {
            var cuota = await _db.Cuotas.FindAsync(id);

            if (cuota == null)
            {
                return NotFound("No existe esa cuota.");
            }

            if (cuota.Estado == "Pagada")
            {
                return BadRequest("¡Esa cuota ya estaba pagada!");
            }

            // Cambiamos el estado
            cuota.Estado = "Pagada";

            // Guardamos el cambio
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = $"La cuota de {cuota.Mes}/{cuota.Anio} quedó PAGADA." });
        }
    }
}