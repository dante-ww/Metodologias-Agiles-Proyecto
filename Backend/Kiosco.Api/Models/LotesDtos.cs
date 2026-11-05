namespace Kiosco.Api.Models
{
    // Para crear un lote nuevo
    public class CreateLoteRequest
    {
        public int IdProducto { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int Cantidad { get; set; } // Se asigna a CantidadInicial y CantidadActual
    }

    // Para listar lotes (respuesta)
    public class LoteResponse
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public int CantidadInicial { get; set; }
        public int CantidadActual { get; set; }
        public DateTime FechaIngreso { get; set; }
        public bool EstaVencido { get; set; } // Calculado: FechaVencimiento < Hoy
        public int DiasParaVencer { get; set; } // Calculado: (FechaVencimiento - Hoy).Days
    }
}