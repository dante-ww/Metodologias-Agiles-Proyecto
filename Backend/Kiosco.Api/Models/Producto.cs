namespace Kiosco.Api.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = ""; 
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}