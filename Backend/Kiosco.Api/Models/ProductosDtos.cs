namespace Kiosco.Api.Models
{
    public class ProductoResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
    }

    public class ProductoAdminResponse : ProductoResponse
    {
        public decimal PrecioCosto { get; set; }
    }

    public class UpdateProductoRequest
    {
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
    }

    public class CreateProductoRequest
    {
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
    }
}