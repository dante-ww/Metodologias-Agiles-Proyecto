namespace Kiosco.Api.Models
{
    // Vista CAJERO: sin precio de costo
    public class ProductoResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
    }

    // Vista ADMIN: agrega precio de costo
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
        public int? CategoriaId { get; set; }
    }

    public class CreateProductoRequest
    {
        public string Nombre { get; set; } = "";
        public string CodigoBarras { get; set; } = "";
        public decimal PrecioCosto { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int? CategoriaId { get; set; }
    }
}