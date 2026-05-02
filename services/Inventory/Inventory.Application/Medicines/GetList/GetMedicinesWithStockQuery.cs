using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;
using Shared.Contracts.Models;

namespace Inventory.Application.Medicines.GetList;

public record GetMedicinesWithStockQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResponse<List<MedicineWithStockDTO>>>;

public class
    GetMedicinesWithStockQueryHandler : IRequestHandler<GetMedicinesWithStockQuery,
    PagedResponse<List<MedicineWithStockDTO>>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesWithStockQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<PagedResponse<List<MedicineWithStockDTO>>> Handle(GetMedicinesWithStockQuery request,
        CancellationToken ct)
    {
        var medicines = await _context.Medicines.Select(m => new MedicineWithStockDTO
        {
            Id = m.Id,
            Name = m.Name,
            SKU = m.SKU,
            ActiveIngredient = m.ActiveIngredient,
            Unit = m.Unit,
            ImageUrl = m.ImageUrl,
            TotalStock = m.Batches.Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow)
                .Sum(b => b.CurrentQuantity),

            EarliestExpiryDate = m.Batches
                .Where(b => b.CurrentQuantity > 0 && b.ExpiryDate > DateTime.UtcNow && !b.IsDeleted)
                .Min(b => (DateTime?)b.ExpiryDate)
        }).AsNoTracking().ToListAsync(ct);

        return new PagedResponse<List<MedicineWithStockDTO>>(medicines, request.PageNumber, request.PageSize,
            medicines.Count);
    }
}