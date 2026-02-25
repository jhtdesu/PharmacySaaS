using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;

public record GetMedicinesQuery() : IRequest<List<MedicineDto>>;

public record MedicineDto(Guid Id, string Name, string SKU, string Unit);

public class GetMedicinesHandler : IRequestHandler<GetMedicinesQuery, List<MedicineDto>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesHandler(IInventoryDbContext context) => _context = context;

    public async Task<List<MedicineDto>> Handle(GetMedicinesQuery request, CancellationToken ct)
    {
        return await _context.Medicines.Select(m => new MedicineDto(m.Id, m.Name, m.SKU, m.Unit)).ToListAsync(ct);
    }
}