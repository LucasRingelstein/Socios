using System.ComponentModel.DataAnnotations;

namespace GestionSocios.DTOs
{
    public class CrearCuotaDto
    {
        [Required(ErrorMessage = "Debes indicar a qué socio pertenece la cuota")]
        public int SocioId { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "El mes debe ser del 1 al 12")]
        public int Mes { get; set; }

        [Required]
        [Range(2020, 2100, ErrorMessage = "El año debe ser válido")]
        public int Anio { get; set; }

        [Required]
        [Range(1, 999999, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
    }
}