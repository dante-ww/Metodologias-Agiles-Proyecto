using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMINISTRADOR")] // Solo el admin puede listar usuarios
    public class UsuariosController : ControllerBase
    {
        private readonly KioscoContext _context;

        public UsuariosController(KioscoContext context)
        {
            _context = context;
        }

        // GET api/usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new UsuarioResponse
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Rol = u.Rol.ToString()
                })
                .ToListAsync();

            return Ok(usuarios);
        }
    }
}