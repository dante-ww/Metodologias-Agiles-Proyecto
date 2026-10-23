using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly KioscoContext _context;

        public ClientesController(KioscoContext context)
        {
            _context = context;
        }

        // GET api/clientes
        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .Select(c => new ClienteResponse
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    DniCuit = c.DniCuit,
                    RazonSocial = c.RazonSocial,
                    Domicilio = c.Domicilio
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // POST api/clientes
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del cliente no puede estar vacio");
            }

            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                DniCuit = request.DniCuit,
                RazonSocial = request.RazonSocial,
                Domicilio = request.Domicilio
            };

            _context.Clientes.Add(cliente);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Ya existe un cliente con ese DNI/CUIT.");
            }

            return Created($"/api/clientes/{cliente.Id}", new ClienteResponse
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                DniCuit = cliente.DniCuit,
                RazonSocial = cliente.RazonSocial,
                Domicilio = cliente.Domicilio
            });
        }

        // PUT api/clientes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] CreateClienteRequest request)
        {
            // 1. Buscar el cliente
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            // 2. Validaciones (reutilizando lógica del POST)
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del cliente no puede estar vacio");
            }

            // 3. Actualizar propiedades
            cliente.Nombre = request.Nombre;
            cliente.DniCuit = request.DniCuit;
            cliente.RazonSocial = request.RazonSocial;
            cliente.Domicilio = request.Domicilio;

            // 4. Guardar
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Error al actualizar el cliente (posible DNI/CUIT duplicado).");
            }

            return Ok(new ClienteResponse
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                DniCuit = cliente.DniCuit,
                RazonSocial = cliente.RazonSocial,
                Domicilio = cliente.Domicilio
            });
        }
    }
}