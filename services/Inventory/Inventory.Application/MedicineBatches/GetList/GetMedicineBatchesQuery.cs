using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Application.DTOs.MedicineBatches;

namespace Inventory.Application.MedicineBatches.GetList;

public record GetMedicineBatchesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<MedicineBatchDTO>>>;

public class GetMedicineBatchesQueryHandler : IRequestHandler<GetMedicineBatchesQuery, PagedResponse<List<MedicineBatchDTO>>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineBatchesQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<PagedResponse<List<MedicineBatchDTO>>> Handle(GetMedicineBatchesQuery request, CancellationToken ct)
    {
        var totalRecords = await _context.Batches.CountAsync(ct);

        var batches = await _context.Batches
            .OrderBy(b => b.ExpiryDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new MedicineBatchDTO(b.Id, b.BatchNumber, b.ExpiryDate, b.OriginalQuantity))
            .ToListAsync(ct);

        return new PagedResponse<List<MedicineBatchDTO>>(batches, request.PageNumber, request.PageSize, totalRecords);
    }
}