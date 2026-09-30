using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Cualquier usuario logueado puede ver la lista (para el select del front)
    public class CategoriasController : ControllerBase
    {
        private readonly KioscoContext _context;

        public CategoriasController(KioscoContext context)
        {
            _context = context;
        }

        // GET api/categorias
        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Select(c => new CategoriaResponse
                {
                    Id = c.Id,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // POST api/categorias
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")] // Solo el admin crea categorias
        public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre de la categoria no puede estar vacio");
            }

            var categoria = new Categoria
            {
                Nombre = request.Nombre
            };

            _context.Categorias.Add(categoria);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe una categoria con ese nombre.");
            }

            return Created($"/api/categorias/{categoria.Id}", new CategoriaResponse
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            });
        }
    }
}