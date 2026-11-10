using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LotesController : ControllerBase
    {
        private readonly KioscoContext _context;

        public LotesController(KioscoContext context)
        {
            _context = context;
        }

        // ========================================
        // GET api/lotes
        // Listar todos los lotes o filtrar por producto.
        // Útil para ver stock detallado por vencimiento.
        // Autorización: Cualquier usuario logueado.
        // ========================================
        [HttpGet]
        public async Task<IActionResult> GetLotes([FromQuery] int? idProducto)
        {
            var query = _context.LotesProductos
                .AsNoTracking()
                .Include(l => l.Producto)
                .AsQueryable();

            if (idProducto.HasValue)
            {
                query = query.Where(l => l.IdProducto == idProducto.Value);
            }

            // Se traen solo columnas crudas: sin aritmética de fechas en SQL
            var lotes = await query
                .OrderBy(l => l.FechaVencimiento)
                .Select(l => new
                {
                    l.Id,
                    l.IdProducto,
                    NombreProducto = l.Producto != null ? l.Producto.Nombre : "",
                    l.FechaVencimiento,
                    l.CantidadInicial,
                    l.CantidadActual,
                    l.FechaIngreso
                })
                .ToListAsync();

            // Derivados calculados en memoria (fuera de la consulta SQL)
            var respuesta = lotes.Select(l => new LoteResponse
            {
                Id = l.Id,
                IdProducto = l.IdProducto,
                NombreProducto = l.NombreProducto,
                FechaVencimiento = l.FechaVencimiento,
                CantidadInicial = l.CantidadInicial,
                CantidadActual = l.CantidadActual,
                FechaIngreso = l.FechaIngreso,
                EstaVencido = l.FechaVencimiento < DateTime.Today,
                DiasParaVencer = (l.FechaVencimiento - DateTime.Today).Days
            }).ToList();

            return Ok(respuesta);
        }

        // ========================================
        // GET api/lotes/vencimientos-proximos
        // Alerta de lotes que vencen en los próximos X días.
        // Parámetro query: dias (default 30).
        // Autorización: Cualquier usuario logueado (cajeros necesitan ver esto).
        // ========================================
        [HttpGet("vencimientos-proximos")]
        public async Task<IActionResult> GetVencimientosProximos([FromQuery] int dias = 30)
        {
            var fechaLimite = DateTime.Today.AddDays(dias);

            var lotes = await _context.LotesProductos
                .AsNoTracking()
                .Include(l => l.Producto)
                .Where(l => l.FechaVencimiento >= DateTime.Today
                        && l.FechaVencimiento <= fechaLimite
                        && l.CantidadActual > 0)
                .OrderBy(l => l.FechaVencimiento)
                .Select(l => new
                {
                    l.Id,
                    l.IdProducto,
                    NombreProducto = l.Producto != null ? l.Producto.Nombre : "",
                    l.FechaVencimiento,
                    l.CantidadInicial,
                    l.CantidadActual,
                    l.FechaIngreso
                })
                .ToListAsync();

            // Derivados calculados en memoria (fuera de la consulta SQL)
            var respuesta = lotes.Select(l => new LoteResponse
            {
                Id = l.Id,
                IdProducto = l.IdProducto,
                NombreProducto = l.NombreProducto,
                FechaVencimiento = l.FechaVencimiento,
                CantidadInicial = l.CantidadInicial,
                CantidadActual = l.CantidadActual,
                FechaIngreso = l.FechaIngreso,
                EstaVencido = false, // Por definición del filtro, aún no están vencidos
                DiasParaVencer = (l.FechaVencimiento - DateTime.Today).Days
            }).ToList();

            return Ok(respuesta);
        }

        // ========================================
        // POST api/lotes
        // Registrar ingreso de mercadería con lote y vencimiento.
        // Regla de negocio: CantidadActual se inicializa igual a Cantidad.
        // Autorización: Solo ADMINISTRADOR (gestión de inventario).
        // ========================================
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CreateLote([FromBody] CreateLoteRequest request)
        {
            if (request.Cantidad <= 0)
            {
                return BadRequest("La cantidad debe ser mayor a 0.");
            }

            if (request.FechaVencimiento < DateTime.Today)
            {
                return BadRequest("No se puede ingresar mercadería ya vencida.");
            }

            var producto = await _context.Productos.FindAsync(request.IdProducto);
            if (producto == null)
            {
                return NotFound("El producto indicado no existe.");
            }

            var lote = new LoteProducto
            {
                IdProducto = request.IdProducto,
                FechaVencimiento = request.FechaVencimiento,
                CantidadInicial = request.Cantidad,
                CantidadActual = request.Cantidad,
                FechaIngreso = DateTime.Now
            };

            _context.LotesProductos.Add(lote);
            await _context.SaveChangesAsync();

            return Created($"/api/lotes/{lote.Id}", new LoteResponse
            {
                Id = lote.Id,
                IdProducto = lote.IdProducto,
                NombreProducto = producto.Nombre,
                FechaVencimiento = lote.FechaVencimiento,
                CantidadInicial = lote.CantidadInicial,
                CantidadActual = lote.CantidadActual,
                FechaIngreso = lote.FechaIngreso,
                EstaVencido = lote.FechaVencimiento < DateTime.Today,
                DiasParaVencer = (lote.FechaVencimiento - DateTime.Today).Days
            });
        }

        // ========================================
        // DELETE api/lotes/5
        // Eliminar un lote (error de carga o limpieza de datos de prueba).
        // Autorización: Solo ADMINISTRADOR (coherente con el POST).
        // ========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteLote(int id)
        {
            var lote = await _context.LotesProductos.FindAsync(id);
            if (lote == null)
            {
                return NotFound();
            }

            _context.LotesProductos.Remove(lote);
            await _context.SaveChangesAsync();

            // Estándar REST: 204 No Content, igual que DELETE de productos y usuarios
            return NoContent();
        }
    }
}