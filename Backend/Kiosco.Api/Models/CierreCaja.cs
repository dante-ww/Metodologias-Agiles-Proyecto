namespace Kiosco.Api.Models
{
    public class CierreCaja
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalTarjeta { get; set; }
        public int CantidadVentas { get; set; }

        // Navegaciones
        public Usuario? Usuario { get; set; }
        public List<Venta> Ventas { get; set; } = new();
    }
}