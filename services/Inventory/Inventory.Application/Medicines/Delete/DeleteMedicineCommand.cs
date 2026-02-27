using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.Medicines.Delete;

public record DeleteMedicineCommand(Guid Id) : IRequest<bool>;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, bool>
{
    private readonly IInventoryDbContext _context;
    public DeleteMedicineCommandHandler(IInventoryDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteMedicineCommand request, CancellationToken ct)
    {
        var medicine = await _context.Medicines.Where(m => m.Id == request.Id).FirstOrDefaultAsync(ct);

        if (medicine == null) return false;

        medicine.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}