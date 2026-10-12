namespace Kiosco.Api.Models
{
    public class CrearVentaRequest
    {
        public string MedioPago { get; set; } = "";
        public decimal MontoRecibido { get; set; }
        public int? IdCliente { get; set; }
        public List<ItemVentaRequest> Items { get; set; } = new();
    }
    public class ItemVentaRequest
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
    }
    public class VentaResponse
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MedioPago { get; set; } = "";
        public decimal Total { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public int? IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public List<ItemVentaResponse> Items { get; set; } = new();
    }

    public class ItemVentaResponse
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
    public class ClienteResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? DniCuit { get; set; }
    }
}