using Inventory.Application.DTOs.MedicineBatches;
using Inventory.Application.MedicineBatches.Create;
using Inventory.Application.MedicineBatches.Delete;
using Inventory.Application.MedicineBatches.GetById;
using Inventory.Application.MedicineBatches.GetList;
using Inventory.Application.MedicineBatches.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicineBatchesController : ControllerBase
{
    private readonly ISender _sender; 

    public MedicineBatchesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:guid}/batches")]
    public async Task<IActionResult> CreateBatch(Guid id, [FromBody] CreateMedicineBatchRequest request)
    {
        var command = new CreateMedicineBatchCommand(
            id,
            request.BatchNumber,
            request.ExpiryDate,
            request.Quantity
        );
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("{id:guid}/batches")]
    public async Task<IActionResult> GetBatchesByMedicineId(Guid id)
    {
        var result = await _sender.Send(new GetMedicineBatchByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("batches")]
    public async Task<IActionResult> GetAllBatches()
    {
        var result = await _sender.Send(new GetMedicineBatchesQuery());
        return Ok(result);
    }

    [HttpPut("batches/{id:guid}")]
    public async Task<IActionResult> UpdateBatch(Guid id, [FromBody] UpdateMedicineBatchCommand command)
    {
        if (id != command.BatchId)
        {
            return BadRequest("ID trong route không khớp với ID trong body");
        }

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpDelete("batches/{id:guid}")]
    public async Task<IActionResult> DeleteBatch(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineBatchCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }
}