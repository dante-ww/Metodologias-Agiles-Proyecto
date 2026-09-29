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

        // DELETE api/usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            // 1. Buscar el usuario
            var usuario = await _context.Usuarios.FindAsync(id);

            // 2. Si no existe, 404
            if (usuario == null)
            {
                return NotFound();
            }

            // 3. Marcar para borrar y guardar
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            // 4. Estándar REST: 204 No Content
            return NoContent();
        }
    }
}