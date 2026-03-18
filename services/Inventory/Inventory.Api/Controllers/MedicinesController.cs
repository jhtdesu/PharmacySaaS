using Shared.Contracts.Models;
using Inventory.Application.DTOs.Medicines;
using Inventory.Application.Medicines.Create;
using Inventory.Application.Medicines.Delete;
using Inventory.Application.Medicines.Dispense;
using Inventory.Application.Medicines.GetById;
using Inventory.Application.Medicines.GetList;
using Inventory.Application.Medicines.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Inventory.Application.Medicines.Queries.GetLowStock;
using Microsoft.AspNetCore.Authorization;
using Inventory.Application.Medicines.Checkout;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicineRequestDTO request)
    {
        var command = new UpdateMedicineCommand(
            id,
            request.Name,
            request.SKU,
            request.ActiveIngredient,
            request.Unit
        );

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
    public async Task<ActionResult<PagedResponse<List<MedicineWithStockDTO>>>> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetMedicinesWithStockQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);
        
        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<PagedResponse<List<LowStockMedicineDTO>>>> GetLowStockMedicines([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetLowStockMedicinesQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);
        
        return Ok(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutCommand command)
    {
        try
        {
            var receiptNumber = await _sender.Send(command);
            return Ok(new { Message = "Sale completed successfully.", ReceiptNumber = receiptNumber });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}