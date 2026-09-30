namespace Kiosco.Api.Models
{
    public class CategoriaResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class CreateCategoriaRequest
    {
        public string Nombre { get; set; } = "";
    }
}