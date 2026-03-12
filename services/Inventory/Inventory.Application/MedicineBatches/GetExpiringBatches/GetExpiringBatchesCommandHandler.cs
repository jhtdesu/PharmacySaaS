using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Models;
using Inventory.Application.DTOs.MedicineBatches;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.MedicineBatches.Queries.GetExpiringBatches;

public class GetExpiringBatchesQueryHandler : IRequestHandler<GetExpiringBatchesQuery, BaseResponse<List<ExpiringBatchDTO>>>
{
    private readonly IInventoryDbContext _context;

    public GetExpiringBatchesQueryHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<BaseResponse<List<ExpiringBatchDTO>>> Handle(GetExpiringBatchesQuery request, CancellationToken cancellationToken)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(request.DaysThreshold);

        var expiringBatches = await _context.Batches
            .AsNoTracking()
            .Include(b => b.Medicine) 
            .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate <= expiryThreshold)
            .OrderBy(b => b.ExpiryDate) 
            .Select(b => new ExpiringBatchDTO
            {
                BatchId = b.Id,
                MedicineName = b.Medicine.Name,
                BatchNumber = b.BatchNumber,
                RemainingQuantity = b.CurrentQuantity,
                ExpiryDate = b.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        return new BaseResponse<List<ExpiringBatchDTO>>(expiringBatches, "Expiring batches retrieved successfully.");
    }
}