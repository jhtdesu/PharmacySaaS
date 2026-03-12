using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.Common.Models;
using Inventory.Application.DTOs.Medicines;

namespace Inventory.Application.Medicines.GetList;

public record GetMedicinesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<MedicineDTO>>>;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, PagedResponse<List<MedicineDTO>>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<PagedResponse<List<MedicineDTO>>> Handle(GetMedicinesQuery request, CancellationToken ct)
    {
        var totalRecords = await _context.Medicines.CountAsync(ct);

        var medicines = await _context.Medicines
            .OrderBy(m => m.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MedicineDTO(m.Id, m.Name, m.SKU, m.Unit))
            .ToListAsync(ct);

        return new PagedResponse<List<MedicineDTO>>(medicines, request.PageNumber, request.PageSize, totalRecords);
    }
}