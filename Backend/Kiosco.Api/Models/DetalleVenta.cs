namespace Kiosco.Api.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Navegaciones
        public Venta? Venta { get; set; }
        public Producto? Producto { get; set; }
    }
}