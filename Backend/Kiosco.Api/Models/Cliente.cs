namespace Kiosco.Api.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? DniCuit { get; set; }
        public string? RazonSocial { get; set; }
        public string? Domicilio { get; set; }
    }
}