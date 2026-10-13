using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tanto cajeros como admins pueden ver ventas
    public class VentasController : ControllerBase
    {
        private readonly KioscoContext _context;

        public VentasController(KioscoContext context)
        {
            _context = context;
        }

        // GET api/ventas
        // Lista todas las ventas con info básica
        [HttpGet]
        public async Task<IActionResult> GetVentas()
        {
            var ventas = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .Include(v => v.Detalle)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .Select(v => new VentaResponse
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    MedioPago = v.MedioPago,
                    Total = v.Total,
                    MontoRecibido = v.MontoRecibido,
                    Vuelto = v.Vuelto,
                    IdUsuario = v.IdUsuario,
                    NombreUsuario = v.Usuario != null ? v.Usuario.Nombre : "",
                    IdCliente = v.IdCliente,
                    NombreCliente = v.Cliente != null ? v.Cliente.Nombre : "",
                    Items = v.Detalle.Select(d => new ItemVentaResponse
                    {
                        IdProducto = d.IdProducto,
                        NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // GET api/ventas/5
        // Detalle completo de una venta específica (ticket)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.Cliente)
                .Include(v => v.Detalle)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                return NotFound();
            }

            var response = new VentaResponse
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                MedioPago = venta.MedioPago,
                Total = venta.Total,
                MontoRecibido = venta.MontoRecibido,
                Vuelto = venta.Vuelto,
                IdUsuario = venta.IdUsuario,
                NombreUsuario = venta.Usuario != null ? venta.Usuario.Nombre : "",
                IdCliente = venta.IdCliente,
                NombreCliente = venta.Cliente != null ? venta.Cliente.Nombre : "",
                Items = venta.Detalle.Select(d => new ItemVentaResponse
                {
                    IdProducto = d.IdProducto,
                    NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            return Ok(response);
        }

        // POST api/ventas
        // Cobra una venta, calcula vuelto, descuenta stock en transacción
        [HttpPost]
        public async Task<IActionResult> CreateVenta([FromBody] CrearVentaRequest request)
        {
            // 1. Validaciones básicas del request
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("La venta debe tener al menos un producto.");
            }

            if (!Enum.TryParse<MedioPago>(request.MedioPago, true, out var medioPago))
            {
                return BadRequest("Medio de pago inválido. Use EFECTIVO o TARJETA.");
            }

            // 2. Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Calcular total y validar stock de cada ítem
                decimal total = 0;
                var detalles = new List<DetalleVenta>();

                foreach (var item in request.Items)
                {
                    if (item.Cantidad <= 0)
                    {
                        return BadRequest($"La cantidad del producto {item.IdProducto} debe ser mayor a 0.");
                    }

                    // 4. Ejecutar actualización atómica directa en MySQL
                    var filasAfectadas = await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE productos SET stock = stock - {item.Cantidad} WHERE id = {item.IdProducto} AND stock >= {item.Cantidad}"
                    );

                    // 5. Verificar si la actualización fue exitosa
                    if (filasAfectadas == 0)
                    {
                        var productoFallido = await _context.Productos.FindAsync(item.IdProducto);
                        string nombreProducto = productoFallido?.Nombre ?? $"ID {item.IdProducto}";

                        return BadRequest($"Stock insuficiente para {nombreProducto}. Intente nuevamente.");
                    }

                    // 6. el stock se descontó con éxito de forma atómica.
                    var producto = await _context.Productos.FindAsync(item.IdProducto);

                    if (producto == null)
                    {
                        return StatusCode(500, $"Error interno: No se pudo recuperar el producto {item.IdProducto} tras actualizar stock.");
                    }

                    decimal subtotal = producto.PrecioVenta * item.Cantidad;
                    total += subtotal;

                    // 7. Preparar el detalle de venta
                    detalles.Add(new DetalleVenta
                    {
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = subtotal
                    });
                }

                // 8. Calcular vuelto según medio de pago
                decimal montoRecibido = request.MontoRecibido;
                decimal vuelto = 0;

                if (medioPago == MedioPago.TARJETA)
                {
                    // Convención: en tarjeta se cobra el exacto, no hay vuelto
                    montoRecibido = total;
                    vuelto = 0;
                }
                else // EFECTIVO
                {
                    if (montoRecibido < total)
                    {
                        return BadRequest($"Monto recibido insuficiente. Total: {total}, Recibido: {montoRecibido}.");
                    }
                    vuelto = montoRecibido - total;
                }

                // 9. Determinar ID del cliente (usar 1 = Consumidor Final si no viene)
                int idCliente = request.IdCliente ?? 1;

                // Verificar que el cliente exista (evita error de FK)
                if (!await _context.Clientes.AnyAsync(c => c.Id == idCliente))
                {
                    return BadRequest("El cliente indicado no existe.");
                }

                // 10. Obtener ID del usuario logueado (cajero) desde el token JWT
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuario))
                {
                    return Unauthorized("No se pudo identificar al usuario.");
                }

                // 11. Crear la venta
                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    MedioPago = medioPago.ToString(), // Guardar como string "EFECTIVO"/"TARJETA"
                    Total = total,
                    MontoRecibido = montoRecibido,
                    Vuelto = vuelto,
                    IdUsuario = idUsuario,
                    IdCliente = idCliente,
                    Detalle = detalles
                };

                _context.Ventas.Add(venta);

                // 12. Guardar todo (venta + detalles + stock actualizado)
                await _context.SaveChangesAsync();

                // 13. Confirmar transacción
                await transaction.CommitAsync();

                // 14. Devolver el ticket completo (recargar con includes para el response)
                var ventaCreada = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Usuario)
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalle)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == venta.Id);

                if (ventaCreada == null)
                {
                    // Esto no debería pasar nunca (acabamos de guardarla), pero el compilador lo exige
                    return StatusCode(500, "Error interno: la venta se guardó pero no se pudo recuperar.");
                }

                var response = new VentaResponse
                {
                    Id = ventaCreada.Id,
                    Fecha = ventaCreada.Fecha,
                    MedioPago = ventaCreada.MedioPago,
                    Total = ventaCreada.Total,
                    MontoRecibido = ventaCreada.MontoRecibido,
                    Vuelto = ventaCreada.Vuelto,
                    IdUsuario = ventaCreada.IdUsuario,
                    NombreUsuario = ventaCreada.Usuario != null ? ventaCreada.Usuario.Nombre : "",
                    IdCliente = ventaCreada.IdCliente,
                    NombreCliente = ventaCreada.Cliente != null ? ventaCreada.Cliente.Nombre : "",
                    Items = ventaCreada.Detalle.Select(d => new ItemVentaResponse
                    {
                        IdProducto = d.IdProducto,
                        NombreProducto = d.Producto != null ? d.Producto.Nombre : "",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, response);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return Conflict("Error al guardar la venta. Intente nuevamente.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error interno al procesar la venta.");
            }
        }
    }
}