using Inventory.Api.DTOs;
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
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sender.Send(new GetMedicinesQuery());
        return Ok(result);
    }
    [HttpPost("{id:guid}/batches")]
    public async Task<IActionResult> CreateBatch(Guid id, [FromBody] CreateMedicineBatchRequest request)
    {
        var result = await _sender.Send(request);
        return Ok(result);
    }
}