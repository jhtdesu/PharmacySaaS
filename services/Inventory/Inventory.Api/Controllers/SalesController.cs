using Shared.Contracts.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory.Application.DTOs.Sales;
using Inventory.Application.Sales.GetById;
using Inventory.Application.Sales.GetList;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISender _sender; 

    public SalesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<List<SaleDTO>>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _sender.Send(new GetSalesQuery(pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetSaleByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(new BaseResponse<object>(result, "Sale retrieved."));
    }
}