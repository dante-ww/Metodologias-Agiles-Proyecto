using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Toda la API de productos exige token válido
    public class ProductosController : ControllerBase
    {
        private readonly KioscoContext _context;

        public ProductosController(KioscoContext context)
        {
            _context = context;
        }

        // Busca el nombre de la categoria (o null si el producto no tiene)
        private async Task<string?> ObtenerNombreCategoria(int? categoriaId)
        {
            if (categoriaId == null) return null;

            return await _context.Categorias
                .Where(c => c.Id == categoriaId)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync();
        }

        // GET api/productos
        // el CAJERO no debe ver precios de costo; el ADMINISTRADOR sí.
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _context.Productos.AsNoTracking().Include(p => p.Categoria).ToListAsync();

            // Vista admin
            if (User.IsInRole("ADMINISTRADOR"))
            {
                return Ok(productos.Select(p => new ProductoAdminResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarras = p.CodigoBarras,
                    Stock = p.Stock,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,
                    PrecioCosto = p.PrecioCosto,
                    PrecioVenta = p.PrecioVenta
                }));
            }

            // Vista cajero
            return Ok(productos.Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                CodigoBarras = p.CodigoBarras,
                Stock = p.Stock,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,
                PrecioVenta = p.PrecioVenta

            }));
        }

        // PUT api/productos/5
        // Solo ADMIN
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] UpdateProductoRequest request)
        {
            // 1. Buscamos el producto original
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

           if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del producto no puede estar vacio");
            }

            if (string.IsNullOrWhiteSpace(request.CodigoBarras))
            {
                return BadRequest("El código de barras es obligatorio");
            }

            if (!request.CodigoBarras.All(char.IsDigit))
            {
                return BadRequest("El código de barras debe contener solo numeros.");
            }

            // Si mandan una categoria, verificamos que exista antes de guardar
            if (request.CategoriaId != null &&
                !await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId))
            {
                return BadRequest("La categoria indicada no existe.");
            }
            // 3. Pasamos los datos del request al producto
            producto.PrecioVenta = request.PrecioVenta;
            producto.Stock = request.Stock;
            producto.PrecioCosto = request.PrecioCosto;
            producto.Nombre = request.Nombre;
            producto.CodigoBarras = request.CodigoBarras;
            producto.CategoriaId = request.CategoriaId;

            // 4. Guardamos en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Error al actualizar.");
            }

            var categoriaNombre = await ObtenerNombreCategoria(producto.CategoriaId);

            // 5. Devolvemos la vista completa
            return Ok(new ProductoAdminResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBarras = producto.CodigoBarras,
                PrecioVenta = producto.PrecioVenta,
                PrecioCosto = producto.PrecioCosto,
                Stock = producto.Stock,
                CategoriaId = producto.CategoriaId,
                CategoriaNombre = categoriaNombre
            });
        }

        // POST api/productos
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> CreateProducto([FromBody] CreateProductoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del producto no puede estar vacio");
            }

            if (string.IsNullOrWhiteSpace(request.CodigoBarras))
            {
                return BadRequest("El código de barras es obligatorio");
            }

            if (!request.CodigoBarras.All(char.IsDigit))
            {
                return BadRequest("El código de barras debe contener solo numeros.");
            }

            // Si mandan una categoria, verificamos que exista antes de guardar
            if (request.CategoriaId != null &&
                !await _context.Categorias.AnyAsync(c => c.Id == request.CategoriaId))
            {
                return BadRequest("La categoria indicada no existe.");
            }

            var producto = new Producto
            {
                Nombre = request.Nombre,
                CodigoBarras = request.CodigoBarras,
                PrecioCosto = request.PrecioCosto,
                Stock = request.Stock,
                PrecioVenta = request.PrecioVenta,
                CategoriaId = request.CategoriaId
            };

            _context.Productos.Add(producto);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Si falla por UNIQUE constraint de codigo_barras
                return Conflict("Ya existe un producto con ese código de barras.");
            }

            var categoriaNombre = await ObtenerNombreCategoria(producto.CategoriaId);

            // Después de SaveChanges, EF rellena producto.Id con el autoincremental de MySQL
            return Created($"/api/productos/{producto.Id}", new ProductoAdminResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBarras = producto.CodigoBarras,
                Stock = producto.Stock,
                CategoriaId = producto.CategoriaId,
                CategoriaNombre = categoriaNombre,
                PrecioVenta = producto.PrecioVenta,
                PrecioCosto = producto.PrecioCosto
            });
        }

        // DELETE api/productos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            // 1. Buscar el producto
            var producto = await _context.Productos.FindAsync(id);

            // 2. Si no existe, 404
            if (producto == null)
            {
                return NotFound();
            }

            // 3. Marcar para borrar y guardar
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            // 4. Estándar REST: 204 No Content (borrado exitoso, sin cuerpo)
            return NoContent();
        }
    }
}