using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Medicines.Update;

internal sealed class UpdateMedicineCommandHandler
    : IRequestHandler<UpdateMedicineCommand, Guid>
{
    private readonly IInventoryDbContext _context;

    public UpdateMedicineCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (medicine == null)
        {
            throw new Exception($"Không tìm thấy thuốc với ID: {request.Id}");
        }

        medicine.Name = request.Name;
        medicine.SKU = request.SKU;
        medicine.ActiveIngredient = request.ActiveIngredient;
        medicine.Unit = request.Unit;
        medicine.ImageUrl = request.ImageUrl;

        _context.Medicines.Update(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return medicine.Id;
    }
}
