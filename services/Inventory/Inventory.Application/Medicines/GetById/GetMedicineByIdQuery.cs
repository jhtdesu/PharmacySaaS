using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Medicines;

public record GetMedicineByIdQuery(Guid Id) : IRequest<MedicineDTO?>;

public class GetMedicineByIdHandler : IRequestHandler<GetMedicineByIdQuery, MedicineDTO?>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineByIdHandler(IInventoryDbContext context) => _context = context;

    public async Task<MedicineDTO?> Handle(GetMedicineByIdQuery request, CancellationToken ct)
    {
        return await _context.Medicines
            .Where(m => m.Id == request.Id)
            .Select(m => new MedicineDTO(m.Id, m.Name, m.SKU, m.Unit))
            .FirstOrDefaultAsync(ct);
    }
}