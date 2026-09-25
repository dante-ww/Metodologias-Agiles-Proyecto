using Kiosco.Api.Models;
using Kiosco.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Validar credenciales con BCrypt
            var usuario = await _authService.ValidarCredenciales(request.Nombre, request.Password);

            // 2. Si las credenciales son inválidas, devolver 401 Unauthorized
            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Nombre de usuario o contraseña incorrectos." });
            }

            // 3. Generar token JWT
            var token = _authService.GenerarToken(usuario);

            // 4. Devolver 200 OK con el token y los datos del usuario
            return Ok(new LoginResponse
            {
                Token = token,
                id = usuario.Id,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol.ToString()
            });
        }

        // POST /api/auth/createUser
        [HttpPost("createUser")]
        // [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            // 1. Intentar crear el usuario
            bool creado = await _authService.CrearUsuario(request.Nombre, request.Password, request.Rol);

            // 2. Si ya existe un usuario con ese nombre, devolver 409 Conflict
            if (!creado)
            {
                return Conflict(new { mensaje = "Ya existe un usuario con ese nombre." });
            }

            // 3. Devolver 201 Created
            return CreatedAtAction(nameof(Login), new { mensaje = "Usuario creado exitosamente." });
        }
    }
}