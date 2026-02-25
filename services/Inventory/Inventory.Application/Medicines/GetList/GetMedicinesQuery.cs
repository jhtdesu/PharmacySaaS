using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;

public record GetMedicinesQuery() : IRequest<List<MedicineDTO>>;

public class GetMedicinesHandler : IRequestHandler<GetMedicinesQuery, List<MedicineDTO>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicinesHandler(IInventoryDbContext context) => _context = context;

    public async Task<List<MedicineDTO>> Handle(GetMedicinesQuery request, CancellationToken ct)
    {
        return await _context.Medicines.Select(m => new MedicineDTO(m.Id, m.Name, m.SKU, m.Unit)).ToListAsync(ct);
    }
}