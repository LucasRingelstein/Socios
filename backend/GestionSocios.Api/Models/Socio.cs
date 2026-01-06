
namespace GestionSocios.Api.Models

{
    public class Socio
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public int Edad { get; set; }
        public string? DNI { get; set; }
        public string? Domicilio { get; set; }
        public string? Actividad { get; set; }
        public string? Sexo { get; set; }
     }
}
