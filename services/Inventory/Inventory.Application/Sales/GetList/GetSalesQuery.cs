using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Shared.Contracts.Models;
using Inventory.Application.DTOs.Sales;
using Inventory.Domain.Entities;

namespace Inventory.Application.Sales.GetList;

public record GetSalesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<SaleDTO>>>;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, PagedResponse<List<SaleDTO>>>
{
    private readonly IInventoryDbContext _context;
    public GetSalesQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<PagedResponse<List<SaleDTO>>> Handle(GetSalesQuery request, CancellationToken ct)
    {
        var totalRecords = await _context.Sales.CountAsync(ct);

        var sales = await _context.Sales.AsNoTracking().Where(s => s.SaleStatus == SaleStatus.Completed).OrderBy(s => s.SaleDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).Select(s => new SaleDTO(s.Id, s.ReceiptNumber, s.SaleDate, s.TotalAmount, s.ProcessedBy)).ToListAsync(ct);

        return new PagedResponse<List<SaleDTO>>(sales, request.PageNumber, request.PageSize, totalRecords);
    }
}