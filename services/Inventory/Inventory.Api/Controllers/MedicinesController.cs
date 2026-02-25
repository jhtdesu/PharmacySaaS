using Inventory.Application.DTOs.MedicineBatches;
using Inventory.Application.Medicines.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly ISender _sender; 

    public MedicinesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicineCommand command)
    {
        var result = await _sender.Send(command);
        
        return Ok(result); 
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetMedicineByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sender.Send(new GetMedicinesQuery());
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineRequest(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:guid}/batches")]
    public async Task<IActionResult> CreateBatch(Guid id, [FromBody] CreateMedicineBatchRequest request)
    {
        var result = await _sender.Send(request);
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
    [HttpDelete("batches/{id:guid}")]
    public async Task<IActionResult> DeleteBatch(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineBatchRequest(id));
        if (!result) return NotFound();
        return NoContent();
    }
}