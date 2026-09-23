using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kiosco.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Kiosco.Api.Services
{
    public class AuthService
    {
        private readonly KioscoContext _context;
        private readonly IConfiguration _config;

        public AuthService(KioscoContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Usuario?> ValidarCredenciales(string nombre, string password)
        {
            // 1. Buscar el usuario por nombre en la base de datos
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre == nombre);

            // 2. Si no existe, devolver null
            if (usuario == null)
            {
                return null;
            }

            // 3. Comparar la contraseña ingresada con el hash almacenado
            bool passwordCorrecta = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

            // 4. Si coincide, devolver el usuario. Si no, devolver null
            if (passwordCorrecta)
            {
                return usuario;
            }

            return null;
        }

        public string GenerarToken(Usuario usuario)
        {
            // 1. Leer configuración JWT desde appsettings
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key");
            var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Falta Jwt:Issuer");
            var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Falta Jwt:Audience");

            // 2. Crear los "claims" (datos que van dentro del token)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            // 3. Crear la clave de firma
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // 4. Crear el token con expiración de 1 hora
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            // 5. Convertir el token a string y devolverlo
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> CrearUsuario(string nombre, string password, Rol rol)
        {
            // 1. Verificar si ya existe un usuario con ese nombre
            var existe = await _context.Usuarios
                .AnyAsync(u => u.Nombre == nombre);

            if (existe)
            {
                return false;
            }

            // 2. Generar el hash de la contraseña usando BCrypt
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            // 3. Crear el objeto Usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                PasswordHash = hash,
                Rol = rol
            };

            // 4. Guardar en la base de datos
            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}