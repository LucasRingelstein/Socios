namespace GestionSocios.DTOs
{
    public class CuotaDto
{
    public int Id { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal Monto { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Datos del Socio para no tener que buscarlo aparte
    public int SocioId { get; set; }
    public string? NombreSocio { get; set; }
}
}