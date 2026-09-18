namespace Kiosco.Api.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public int id { get; set; }
        public string Nombre { get; set; } = "";
        public string Rol { get; set; } = "";
    }
}