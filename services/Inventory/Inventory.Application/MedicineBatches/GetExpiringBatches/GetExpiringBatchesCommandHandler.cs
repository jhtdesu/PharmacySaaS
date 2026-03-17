using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Models;
using Inventory.Application.DTOs.MedicineBatches;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.MedicineBatches.Queries.GetExpiringBatches;

public class GetExpiringBatchesQueryHandler : IRequestHandler<GetExpiringBatchesQuery, PagedResponse<List<ExpiringBatchDTO>>>
{
    private readonly IInventoryDbContext _context;

    public GetExpiringBatchesQueryHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<List<ExpiringBatchDTO>>> Handle(GetExpiringBatchesQuery request, CancellationToken cancellationToken)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(request.DaysThreshold);

        var query = _context.Batches
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .Include(b => b.Medicine)
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate <= expiryThreshold)
            .OrderBy(b => b.ExpiryDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new ExpiringBatchDTO
            {
                BatchId = b.Id,
                MedicineName = b.Medicine.Name,
                BatchNumber = b.BatchNumber,
                RemainingQuantity = b.CurrentQuantity,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<List<ExpiringBatchDTO>>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}