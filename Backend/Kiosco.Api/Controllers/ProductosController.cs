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
                    PrecioVenta = p.PrecioVenta,
                    PrecioCosto = p.PrecioCosto
                }));
            }

            // Vista cajero
            return Ok(productos.Select(p => new ProductoResponse
            {
                Id = p.Id,
                Nombre = p.Nombre,
                PrecioVenta = p.PrecioVenta
            }));
        }

        // PUT api/productos/5
        // Solo el ADMINISTRADOR modifica precios de venta
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> UpdatePrecioVenta(int id, [FromBody] UpdateProductoRequest request)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            producto.PrecioVenta = request.PrecioVenta;
            await _context.SaveChangesAsync();

            return Ok(new ProductoAdminResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                PrecioVenta = producto.PrecioVenta,
                PrecioCosto = producto.PrecioCosto
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

            var producto = new Producto
            {
                Nombre = request.Nombre,
                PrecioCosto = request.PrecioCosto,
                PrecioVenta = request.PrecioVenta
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Después de SaveChanges, EF rellena producto.Id con el autoincremental de MySQL
            return Created($"/api/productos/{producto.Id}", new ProductoAdminResponse
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                PrecioVenta = producto.PrecioVenta,
                PrecioCosto = producto.PrecioCosto
            });
        }
    }
}