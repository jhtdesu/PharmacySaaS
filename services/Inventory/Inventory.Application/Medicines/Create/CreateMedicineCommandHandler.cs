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
        // 1. Khởi tạo Entity từ Request
        var medicine = new Medicine
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = request.SKU,
            ActiveIngredient = request.ActiveIngredient,
            Unit = request.Unit
        };

        // 2. Lưu vào Database
        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Trả về kết quả
        return medicine.Id;
    }
}