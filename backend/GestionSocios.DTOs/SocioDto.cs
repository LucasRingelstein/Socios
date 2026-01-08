using System.ComponentModel.DataAnnotations;
namespace GestionSocios.DTOs
{
    // Cambiamos 'internal' por 'public' para que la API la vea
    public class SocioDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string? Apellido { get; set; }

        public DateTime? FechaDeNacimiento { get; set; }

        [Range(1, 120, ErrorMessage = "Edad invalida")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^[0-9]{7,9}$", ErrorMessage = "El DNI debe tener entre 7 y 9 números")]
        public string? DNI { get; set; }

        [EmailAddress(ErrorMessage = "El formato del Email no es válido")]
        public string? Email { get; set; }

        public string? Domicilio { get; set; }

        public string? Actividad { get; set; }

        public string? Sexo { get; set; }

        public bool Activo { get; set; }

        public DateTime? FechaAlta { get; set; }

        public DateTime? FechaBaja { get; set; }
    }
}

 