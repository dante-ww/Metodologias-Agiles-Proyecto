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

        // GET api/productos
        // el CAJERO no debe ver precios de costo; el ADMINISTRADOR sí.
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _context.Productos.AsNoTracking().ToListAsync();

            // Vista admin
            if (User.IsInRole("ADMINISTRADOR"))
            {
                return Ok(productos.Select(p => new ProductoAdminResponse
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoBarras = p.CodigoBarras,
                    Stock = p.Stock,
                    StockMinimo = p.StockMinimo,
                    StockBajo = p.Stock <= p.StockMinimo, // Lógica de semáforo para usarse facilmente en el front
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
                StockBajo = p.Stock <= p.StockMinimo,
                PrecioVenta = p.PrecioVenta

            }));
        }

        // PUT api/productos/5
        // Admin: Todo. Cajero: Solo PrecioVenta y Stock.
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR, CAJERO")] // 1. Permitimos entrar a ambos
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] UpdateProductoRequest request)
        {
            // 2. Buscamos el producto original
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            bool esCajero = User.IsInRole("CAJERO");

            if (esCajero)
            {
                // 3. Cosas que el cajero no puede tocar
                request.PrecioCosto = producto.PrecioCosto;
                request.StockMinimo = producto.StockMinimo;
                request.Nombre = producto.Nombre;
                request.CodigoBarras = producto.CodigoBarras;
            }

            // 4. Pasamos los datos del request al producto
            producto.PrecioVenta = request.PrecioVenta;
            producto.Stock = request.Stock;
            producto.PrecioCosto = request.PrecioCosto;
            producto.StockMinimo = request.StockMinimo;
            producto.Nombre = request.Nombre;
            producto.CodigoBarras = request.CodigoBarras;

            // 5. Guardamos en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Error al actualizar.");
            }

            // 6. Respuesta según el rol
            if (User.IsInRole("ADMINISTRADOR"))
            {
                // Admin ve todo
                return Ok(new ProductoAdminResponse
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    CodigoBarras = producto.CodigoBarras,
                    PrecioVenta = producto.PrecioVenta,
                    PrecioCosto = producto.PrecioCosto,
                    Stock = producto.Stock,
                    StockMinimo = producto.StockMinimo,
                    StockBajo = producto.Stock <= producto.StockMinimo
                });
            }

            // Cajero ve solo lo básico
            return Ok(new ProductoResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBarras = producto.CodigoBarras,
                PrecioVenta = producto.PrecioVenta,
                Stock = producto.Stock,
                StockBajo = producto.Stock <= producto.StockMinimo
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
                return BadRequest("El código de barras es obligatorio");

            var producto = new Producto
            {
                Nombre = request.Nombre,
                CodigoBarras = request.CodigoBarras,
                PrecioCosto = request.PrecioCosto,
                Stock = request.Stock,
                StockMinimo = request.StockMinimo,
                PrecioVenta = request.PrecioVenta
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


            // Después de SaveChanges, EF rellena producto.Id con el autoincremental de MySQL
            return Created($"/api/productos/{producto.Id}", new ProductoAdminResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                CodigoBarras = producto.CodigoBarras,
                Stock = producto.Stock,
                StockMinimo = producto.StockMinimo,
                StockBajo = producto.Stock <= producto.StockMinimo,
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