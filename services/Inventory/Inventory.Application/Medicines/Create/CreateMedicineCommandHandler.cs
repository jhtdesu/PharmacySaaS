using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;

namespace Inventory.Application.Medicines.Create;

internal sealed class CreateMedicineCommandHandler 
    : IRequestHandler<CreateMedicineCommand, Guid>
{
    private readonly IInventoryDbContext _context;

    public CreateMedicineCommandHandler(IInventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = new Medicine
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = request.SKU,
            ActiveIngredient = request.ActiveIngredient,
            Unit = request.Unit
        };

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return medicine.Id;
    }
}