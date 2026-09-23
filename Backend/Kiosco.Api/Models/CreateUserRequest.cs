namespace Kiosco.Api.Models
{
    public class CreateUserRequest
    {
        public string Nombre { get; set; } = "";
        public string Password { get; set; } = "";
        public Rol Rol { get; set; }
    }
}