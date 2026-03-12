using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Models;

namespace Inventory.Application.Medicines.Queries.GetLowStock;

public class GetLowStockMedicinesQueryHandler : IRequestHandler<GetLowStockMedicinesQuery, PagedResponse<List<LowStockMedicineDTO>>>
{
    private readonly IInventoryDbContext _context;

    public GetLowStockMedicinesQueryHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<List<LowStockMedicineDTO>>> Handle(GetLowStockMedicinesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Medicines.AsNoTracking()
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.SKU,
                m.Unit,
                m.LowStockLevel,
                TotalStock = m.Batches
                    .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
                    .Sum(b => b.CurrentQuantity)
            })
            .Where(m => m.TotalStock <= m.LowStockLevel)
            .OrderBy(m => m.TotalStock);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new LowStockMedicineDTO
            {
                MedicineId = m.Id,
                Name = m.Name,
                SKU = m.SKU,
                Unit = m.Unit,
                LowStockLevel = m.LowStockLevel,
                TotalCurrentStock = m.TotalStock
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<List<LowStockMedicineDTO>>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount
        );
    }
}