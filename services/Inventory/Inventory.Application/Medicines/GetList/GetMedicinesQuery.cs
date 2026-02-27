using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;

namespace Inventory.Application.Medicines.GetList;

public record GetMedicinesQuery() : IRequest<List<MedicineDTO>>;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, List<MedicineDTO>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<List<MedicineDTO>> Handle(GetMedicinesQuery request, CancellationToken ct)
    {
        return await _context.Medicines.Select(m => new MedicineDTO(m.Id, m.Name, m.SKU, m.Unit)).ToListAsync(ct);
    }
}