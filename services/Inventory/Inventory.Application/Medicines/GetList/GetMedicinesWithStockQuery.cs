using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;
using Inventory.Application.Common.Models;

namespace Inventory.Application.Medicines.GetList;

public record GetMedicinesWithStockQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<MedicineWithStockDto>>>;

public class GetMedicinesWithStockQueryHandler : IRequestHandler<GetMedicinesWithStockQuery, PagedResponse<List<MedicineWithStockDto>>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesWithStockQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<PagedResponse<List<MedicineWithStockDto>>> Handle(GetMedicinesWithStockQuery request, CancellationToken ct)
    {
        var medicines = await _context.Medicines.Select(m => new MedicineWithStockDto
        {
            Id = m.Id,
            Name = m.Name,
            SKU = m.SKU,
            Unit = m.Unit,
            TotalStock = m.Batches.Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow).Sum(b => b.CurrentQuantity),

            EarliestExpiryDate = m.Batches.Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow).Min(b => (DateTime?)b.ExpiryDate) 
        }).ToListAsync(ct);

        return new PagedResponse<List<MedicineWithStockDto>>(medicines, request.PageNumber, request.PageSize, medicines.Count);
    }
}