using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;

public record DeleteMedicineRequest(Guid Id) : IRequest<bool>;

public class DeleteMedicineHandler : IRequestHandler<DeleteMedicineRequest, bool>
{
    private readonly IInventoryDbContext _context;
    public DeleteMedicineHandler(IInventoryDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteMedicineRequest request, CancellationToken ct)
    {
        var medicine = await _context.Medicines.Where(m => m.Id == request.Id).FirstOrDefaultAsync(ct);

        if (medicine == null) return false;

        medicine.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}