namespace Kiosco.Api.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}