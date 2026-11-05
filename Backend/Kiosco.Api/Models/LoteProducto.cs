namespace Kiosco.Api.Models
{
    public class LoteProducto
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CantidadInicial { get; set; }
        public int CantidadActual { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // Navegación
        public Producto? Producto { get; set; }
    }
}