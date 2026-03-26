using Inventory.Application.DTOs.MedicineBatches;
using Shared.Contracts.Models;
using Inventory.Application.MedicineBatches.Create;
using Inventory.Application.MedicineBatches.Delete;
using Inventory.Application.MedicineBatches.GetById;
using Inventory.Application.MedicineBatches.GetList;
using Inventory.Application.MedicineBatches.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Inventory.Application.MedicineBatches.Queries.GetExpiringBatches;
using Inventory.Application.MedicineBatches.Commands.AdjustStock;
using Microsoft.AspNetCore.Authorization;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicineBatchesController : ControllerBase
{
    private readonly ISender _sender; 

    public MedicineBatchesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:guid}/batches")]
    public async Task<ActionResult<BaseResponse<object>>> CreateBatch(Guid id, [FromBody] CreateMedicineBatchRequest request)
    {
        var command = new CreateMedicineBatchCommand(
            id,
            request.BatchNumber,
            request.ExpiryDate,
            request.Quantity
        );
        var result = await _sender.Send(command);
        return Ok(new BaseResponse<object>(result, "Medicine batch created successfully."));
    }

    [HttpGet("{id:guid}/batches")]
    public async Task<ActionResult<BaseResponse<object>>> GetBatchesByMedicineId(Guid id)
    {
        var result = await _sender.Send(new GetMedicineBatchByIdQuery(id));
        if (result == null) 
            return BadRequest(new BaseResponse<object>("Batches not found."));
        return Ok(new BaseResponse<object>(result, "Batches retrieved successfully."));
    }

    [HttpGet("batches")]
    public async Task<ActionResult<PagedResponse<List<MedicineBatchDTO>>>> GetAllBatches([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _sender.Send(new GetMedicineBatchesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPut("batches/{id:guid}")]
    public async Task<ActionResult<BaseResponse<object>>> UpdateBatch(Guid id, [FromBody] UpdateMedicineBatchRequestDTO request)
    {
        var command = new UpdateMedicineBatchCommand(
            id,
            request.BatchNumber,
            request.ExpiryDate,
            request.Quantity
        );

        var result = await _sender.Send(command);
        return Ok(new BaseResponse<object>(result, "Medicine batch updated successfully."));
    }

    [HttpDelete("batches/{id:guid}")]
    public async Task<ActionResult<BaseResponse<object>>> DeleteBatch(Guid id)
    {
        var result = await _sender.Send(new DeleteMedicineBatchCommand(id));
        if (!result) 
            return NotFound(new BaseResponse<object>("Batch not found."));
        return Ok(new BaseResponse<object>(null!, "Medicine batch deleted successfully."));
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<PagedResponse<List<ExpiringBatchDTO>>>> GetExpiringBatches([FromQuery] int daysThreshold = 90, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetExpiringBatchesQuery(daysThreshold, pageNumber, pageSize);
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/adjust")]
    public async Task<ActionResult<BaseResponse<bool>>> AdjustStock(Guid id, [FromBody] AdjustStockRequest request)
    {
        var command = new AdjustStockCommand(id, request.QuantityToDeduct, request.Reason);
        var result = await _sender.Send(command);
        
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    public record AdjustStockRequest(int QuantityToDeduct, string Reason);
}