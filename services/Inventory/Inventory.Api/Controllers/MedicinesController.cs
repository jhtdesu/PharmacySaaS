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
using Inventory.Application.Sales.GetSaleById;

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
    public async Task<ActionResult<BaseResponse<object>>> Create(CreateMedicineCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(new BaseResponse<object>(result, "Medicine created successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BaseResponse<MedicineDTO>>> GetById(Guid id)
    {
        var result = await _sender.Send(new GetMedicineByIdQuery(id));
        if (result == null) 
            return NotFound(new BaseResponse<MedicineDTO>("Medicine not found."));
        return Ok(new BaseResponse<MedicineDTO>(result, "Medicine retrieved successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<List<MedicineDTO>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _sender.Send(new GetMedicinesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BaseResponse<object>>> Update(Guid id, [FromBody] UpdateMedicineRequestDTO request)
    {
        var command = new UpdateMedicineCommand(
            id,
            request.Name,
            request.SKU,
            request.ActiveIngredient,
            request.Unit
        );

        var result = await _sender.Send(command);
        return Ok(new BaseResponse<object>(result, "Medicine updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<BaseResponse<object>>> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineCommand(id));
        if (!result) 
            return NotFound(new BaseResponse<object>("Medicine not found."));
        return Ok(new BaseResponse<object>(null!, "Medicine deleted successfully."));
    }

    [HttpPost("dispense")]
    public async Task<ActionResult<BaseResponse<object>>> Dispense(DispenseMedicineCommand command)
    {
        await _sender.Send(command);
        return Ok(new BaseResponse<object>(null!, "Medicine dispensed successfully."));
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
    public async Task<ActionResult<BaseResponse<CreatePendingSaleResponseDTO>>> Checkout([FromBody] CreatePendingSaleCommand command)
    {
        try
        {
            var result = await _sender.Send(command);
            return Ok(new BaseResponse<CreatePendingSaleResponseDTO>(result, "Created Order"));
        }
        catch (Exception ex)
        {
            return BadRequest(new BaseResponse<CreatePendingSaleResponseDTO>(ex.Message));
        }
    }

    [HttpPost("checkout/complete")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<object>>> CompleteCheckout([FromBody] CompleteSaleCommand command)
    {
        try
        {
            var receiptNumber = await _sender.Send(command);
            return Ok(new BaseResponse<object>(new { ReceiptNumber = receiptNumber }, "Order completed successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(new BaseResponse<object>(ex.Message));
        }
    }

    [HttpGet("checkout/{saleId}")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<object>>> GetCheckoutStatus(Guid saleId)
    {
        try
        {
            var result = await _sender.Send(new GetSaleByIdQuery(saleId));
            if (result == null)
                return NotFound(new BaseResponse<object>("Sale not found."));
            return Ok(new BaseResponse<object>(result, "Sale retrieved."));
        }
        catch (Exception ex)
        {
            return BadRequest(new BaseResponse<object>(ex.Message));
        }
    }
}