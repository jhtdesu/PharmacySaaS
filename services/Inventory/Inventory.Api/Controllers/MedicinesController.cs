using Inventory.Application.Common.Models;
using Inventory.Application.DTOs.Medicines;
using Inventory.Application.Medicines.Create;
using Inventory.Application.Medicines.Delete;
using Inventory.Application.Medicines.Dispense;
using Inventory.Application.Medicines.GetById;
using Inventory.Application.Medicines.GetList;
using Inventory.Application.Medicines.Update;
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
    public async Task<ActionResult<PagedResponse<List<MedicineDTO>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _sender.Send(new GetMedicinesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID trong route không khớp với ID trong body");
        }

        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("dispense")]
    public async Task<IActionResult> Dispense(DispenseMedicineCommand command)
    {
        await _sender.Send(command);
        return Ok();
    }

    [HttpGet("with-stock")]
    public async Task<ActionResult<PagedResponse<List<MedicineWithStockDto>>>> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetMedicinesWithStockQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);
        
        return Ok(result);
    }
}