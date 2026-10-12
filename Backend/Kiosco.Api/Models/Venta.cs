namespace Kiosco.Api.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MedioPago { get; set; } = "";
        public decimal Total { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto { get; set; }
        public int IdUsuario { get; set; }
        public int? IdCliente { get; set; }

        // Navegaciones
        public Usuario? Usuario { get; set; }
        public Cliente? Cliente { get; set; }
        public List<DetalleVenta> Detalle { get; set; } = new();
    }
}