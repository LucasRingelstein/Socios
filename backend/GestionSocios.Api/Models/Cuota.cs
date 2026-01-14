using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionSocios.Api.Models
{
    public class Cuota
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
        public int Mes { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Anio { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Define formato de moneda en SQL
        public decimal Monto { get; set; }

        public string Estado { get; set; } = "Pendiente"; // Valor por defecto

        // --- RELACIÓN CON SOCIO ---
        // Clave foránea (Foreign Key)
        public int SocioId { get; set; }

        // Propiedad de navegación (Para acceder a los datos del socio desde la cuota)
        [ForeignKey("SocioId")]
        public Socio? Socio { get; set; }
    }
}