using System.ComponentModel.DataAnnotations;

namespace GestionSocios.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}