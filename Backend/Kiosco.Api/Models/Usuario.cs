namespace Kiosco.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string PasswordHash { get; set; }
        public Rol Rol { get; set; }
    }
}