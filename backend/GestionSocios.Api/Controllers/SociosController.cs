using Microsoft.AspNetCore.Mvc;
using GestionSocios.Api.Models;

namespace GestionSocios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SociosController : ControllerBase
    {
        // Lista en memoria para probar sin base de datos por ahora
        private static List<Socio> socios = new List<Socio>
        {
            new Socio { Id = 1, Nombre = "Juan", Apellido = "Perez", Edad = 30, DNI = "12345678", Actividad = "Fútbol" }
        };

        // GET: api/Socios
        [HttpGet]
        public ActionResult<IEnumerable<Socio>> Get()
        {
            return Ok(socios);
        }

        // GET api/Socios/5
        [HttpGet("{id}")]
        public ActionResult<Socio> Get(int id)
        {
            var socio = socios.FirstOrDefault(s => s.Id == id);
            if (socio == null) return NotFound();
            return Ok(socio);
        }

        // POST api/Socios
        [HttpPost]
        public ActionResult Post([FromBody] Socio nuevoSocio)
        {
            nuevoSocio.Id = socios.Max(s => s.Id) + 1;
            socios.Add(nuevoSocio);
            return CreatedAtAction(nameof(Get), new { id = nuevoSocio.Id }, nuevoSocio);
        }

        // PUT api/Socios/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Socio socioActualizado)
        {
            var socio = socios.FirstOrDefault(s => s.Id == id);
            if (socio == null) return NotFound();

            socio.Nombre = socioActualizado.Nombre;
            socio.Apellido = socioActualizado.Apellido;
            socio.Edad = socioActualizado.Edad;
            socio.DNI = socioActualizado.DNI;
            socio.Domicilio = socioActualizado.Domicilio;
            socio.Actividad = socioActualizado.Actividad;
            socio.Sexo = socioActualizado.Sexo;

            return NoContent();
        }

        // DELETE api/Socios/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var socio = socios.FirstOrDefault(s => s.Id == id);
            if (socio == null) return NotFound();

            socios.Remove(socio);
            return NoContent();
        }
    }
}