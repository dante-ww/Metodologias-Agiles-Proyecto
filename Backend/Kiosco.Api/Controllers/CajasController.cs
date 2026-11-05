using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    // Ruta base: api/cajas
    // [Authorize] a nivel de clase = todos los endpoints exigen token JWT válido
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CajasController : ControllerBase
    {
        private readonly KioscoContext _context;

        public CajasController(KioscoContext context)
        {
            _context = context;
        }

        // ========================================
        // GET api/cajas
        // Listar el historial de todas las cajas (abiertas y cerradas).
        // Solo ADMINISTRADOR: es información sensible de auditoría/contable.
        // ========================================
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetCajas()
        {
            var cierres = await _context.CierresCaja
                .AsNoTracking()
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaInicio)
                .Select(c => new CierreResponse
                {
                    Id = c.Id,
                    IdUsuario = c.IdUsuario,
                    NombreUsuario = c.Usuario != null ? c.Usuario.Nombre : "",
                    FechaInicio = c.FechaInicio,
                    FechaCierre = c.FechaCierre,
                    TotalEfectivo = c.TotalEfectivo,
                    TotalTarjeta = c.TotalTarjeta,
                    CantidadVentas = c.CantidadVentas
                })
                .ToListAsync();

            return Ok(cierres);
        }

        // ========================================
        // POST api/cajas/abrir
        // Abrir una nueva caja (iniciar un turno de trabajo).
        // Regla de negocio: solo puede haber una caja abierta a la vez.
        // Autorización: cualquier usuario logueado (cajero o admin).
        // ========================================
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja()
        {
            if (await _context.CierresCaja.AnyAsync(c => c.FechaCierre == null))
            {
                return Conflict("Ya hay una caja abierta. Cerrala antes de abrir una nueva.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuario))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var nombreUsuario = await _context.Usuarios
                .Where(u => u.Id == idUsuario)
                .Select(u => u.Nombre)
                .FirstOrDefaultAsync() ?? "";

            var cierre = new CierreCaja
            {
                IdUsuario = idUsuario,
                FechaInicio = DateTime.Now
            };

            _context.CierresCaja.Add(cierre);
            await _context.SaveChangesAsync();

            return Created($"/api/cajas/{cierre.Id}", Mapear(cierre, nombreUsuario));
        }

        // ========================================
        // POST api/cajas/{id}/cerrar
        // Cerrar una caja abierta (finalizar el turno).
        // Calcula totales sumando las ventas del período (desde apertura hasta ahora),
        // las asigna al cierre y marca la hora de cierre.
        // Usa transacción porque toca múltiples registros (cierre + N ventas).
        // Autorización: cualquier usuario logueado (el cajero cierra su propio turno).
        // ========================================
        [HttpPost("{id}/cerrar")]
        public async Task<IActionResult> CerrarCaja(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cierre = await _context.CierresCaja
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cierre == null)
                {
                    return NotFound();
                }

                if (cierre.FechaCierre != null)
                {
                    return Conflict("La caja ya está cerrada.");
                }

                var fechaCierre = DateTime.Now;

                // Ventas del turno: sin asignar a ningún cierre y dentro de la ventana de tiempo
                var ventasDelCierre = await _context.Ventas
                    .Where(v => v.IdCierre == null
                            && v.Fecha >= cierre.FechaInicio
                            && v.Fecha <= fechaCierre)
                    .ToListAsync();

                // Se suma Total (no MontoRecibido) porque el vuelto ya salió del cajón;
                // lo que queda físicamente es el Total de cada venta.
                cierre.TotalEfectivo = ventasDelCierre
                    .Where(v => v.MedioPago == MedioPago.EFECTIVO.ToString())
                    .Sum(v => v.Total);
                cierre.TotalTarjeta = ventasDelCierre
                    .Where(v => v.MedioPago == MedioPago.TARJETA.ToString())
                    .Sum(v => v.Total);
                cierre.CantidadVentas = ventasDelCierre.Count;
                cierre.FechaCierre = fechaCierre;

                foreach (var venta in ventasDelCierre)
                {
                    venta.IdCierre = cierre.Id;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(Mapear(cierre, cierre.Usuario != null ? cierre.Usuario.Nombre : ""));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error interno al cerrar la caja.");
            }
        }

        // Helper privado: transforma entidad a DTO cuando ya está cargada en memoria.
        // No se puede usar dentro de Select() de EF (no se traduce a SQL),
        // por eso el GET usa el Select inline.
        private static CierreResponse Mapear(CierreCaja cierre, string nombreUsuario)
        {
            return new CierreResponse
            {
                Id = cierre.Id,
                IdUsuario = cierre.IdUsuario,
                NombreUsuario = nombreUsuario,
                FechaInicio = cierre.FechaInicio,
                FechaCierre = cierre.FechaCierre,
                TotalEfectivo = cierre.TotalEfectivo,
                TotalTarjeta = cierre.TotalTarjeta,
                CantidadVentas = cierre.CantidadVentas
            };
        }
    }
}