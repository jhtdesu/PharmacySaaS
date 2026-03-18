using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;

namespace Inventory.Application.Medicines.GetById;

public record GetMedicineByIdQuery(Guid Id) : IRequest<MedicineDTO?>;

public class GetMedicineByIdQueryHandler : IRequestHandler<GetMedicineByIdQuery, MedicineDTO?>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineByIdQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<MedicineDTO?> Handle(GetMedicineByIdQuery request, CancellationToken ct)
    {
        return await _context.Medicines
            .AsNoTracking()
            .Where(m => m.Id == request.Id && !m.IsDeleted)
            .Select(m => new MedicineDTO(m.Id, m.Name, m.SKU, m.ActiveIngredient, m.Unit))
            .FirstOrDefaultAsync(ct);
    }
}