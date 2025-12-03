using Adq.Backend.Api.Application.Dto;
using Adq.Backend.Api.Application.Services;
using Adq.Backend.Domain.Models;
using Adq.Backend.Domain.Ports;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adq.Backend.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AcquisitionController : ControllerBase
    {
        private readonly AcquisitionService _service;
        private readonly IAcquisitionRepository _repo;
        private readonly ILogger<AcquisitionController> _logger;

        public AcquisitionController(AcquisitionService service, IAcquisitionRepository repo, ILogger<AcquisitionController> logger)
        {
            _service = service;
            _repo = repo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseApi<IEnumerable<Acquisition>>>> GetAll()
        {
            try
            {
                var list = await _repo.GetAllAsync();
                return Ok(ResponseApi<IEnumerable<Acquisition>>.Ok(list.OrderByDescending(x => x.CreatedAt)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las adquisiciones.");
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al obtener las adquisiciones."));
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseApi<Acquisition>>> GetById(Guid id)
        {
            try
            {
                var a = await _repo.GetByIdAsync(id);
                if (a == null)
                    return NotFound(ResponseApi<string>.Fail("No se encontró la adquisición."));

                return Ok(ResponseApi<Acquisition>.Ok(a, "Adquisición encontrada"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener adquisición {Id}.", id);
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al obtener la adquisición."));
            }
        }

        [HttpGet("{id:guid}/history")]
        public async Task<ActionResult<ResponseApi<IEnumerable<HistoryEntry>>>> GetHistory(Guid id)
        {
            try
            {
                var h = await _repo.GetHistoryAsync(id);
                return Ok(ResponseApi<IEnumerable<HistoryEntry>>.Ok(h, "Historial obtenido"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de adquisición {Id}.", id);
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al obtener el historial."));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseApi<Acquisition>>> Create([FromBody] AcquisitionCreateDto dto)
        {
            try
            {
                var created = await _service.CreateAsync(dto);

                var newData = new
                {
                    dto.Budget,
                    dto.Unit,
                    dto.Type,
                    dto.Quantity,
                    dto.UnitValue,
                    dto.AcquisitionDate,
                    dto.Supplier,
                    dto.Documentation
                };

                var historyEntry = new HistoryEntry
                {
                    Id = Guid.NewGuid(),
                    AcquisitionId = created.Id,
                    Action = "INSERT",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { After = newData }),
                    Timestamp = DateTime.UtcNow,
                    User = User?.Identity?.Name ?? "Sistema"
                };

                await _repo.AddHistoryAsync(historyEntry);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ResponseApi<Acquisition>.Ok(created, "Creado correctamente"));
            }
            catch (ArgumentException aex)
            {
                _logger.LogWarning(aex, "Validación inválida al crear adquisición.");
                return BadRequest(ResponseApi<string>.Fail(aex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear adquisición.");
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al crear la adquisición."));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseApi<string>>> Update(Guid id, [FromBody] AcquisitionUpdateDto dto)
        {
            try
            {
                var exists = await _repo.GetByIdAsync(id);
                if (exists == null)
                    return NotFound(ResponseApi<string>.Fail("No se encontró la adquisición a actualizar."));



                // --- 1. Crear historial de modificación ---
                var oldData = new
                {
                    exists.Budget,
                    exists.Unit,
                    exists.Type,
                    exists.Quantity,
                    exists.UnitValue,
                    exists.AcquisitionDate,
                    exists.Supplier,
                    exists.Documentation
                   
                    
                };

                var newData = new
                {
                    dto.Budget,
                    dto.Unit,
                    dto.Type,
                    dto.Quantity,
                    dto.UnitValue,
                    dto.AcquisitionDate,
                    dto.Supplier,
                    dto.Documentation
                };

                var historyEntry = new HistoryEntry
                {
                    Id = Guid.NewGuid(),
                    AcquisitionId = id,
                    Action = "UPDATE",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { Before = oldData, After = newData }),
                    Timestamp = DateTime.UtcNow,
                    User = User?.Identity?.Name ?? "Sistema"
                };

                await _repo.AddHistoryAsync(historyEntry);

                // --- 2. Actualizar entidad ---
                await _service.UpdateAsync(id, dto);
                return Ok(ResponseApi<string>.Ok("Actualizado correctamente"));
            }
            catch (ArgumentException aex)
            {
                _logger.LogWarning(aex, "Validación inválida al actualizar {Id}.", id);
                return BadRequest(ResponseApi<string>.Fail(aex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar adquisición {Id}.", id);
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al actualizar la adquisición."));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseApi<string>>> Delete(Guid id)
        {
            try
            {
                await _service.DeactivateAsync(id, "Eliminado via API");
               

                var historyEntry = new HistoryEntry
                {
                    Id = Guid.NewGuid(),
                    AcquisitionId = id,
                    Action = "DELETE",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { After = id }),
                    Timestamp = DateTime.UtcNow,
                    User = User?.Identity?.Name ?? "Sistema"
                };

                await _repo.AddHistoryAsync(historyEntry);

                return Ok(ResponseApi<string>.Ok("Eliminado correctamente"));
            }
            catch (InvalidOperationException)
            {
                return NotFound(ResponseApi<string>.Fail("No se encontró la adquisición."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar adquisición {Id}.", id);
                return StatusCode(500, ResponseApi<string>.Fail("Error interno al eliminar la adquisición."));
            }
        }
    }
}