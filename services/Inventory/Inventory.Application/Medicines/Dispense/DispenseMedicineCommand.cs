using MediatR;

namespace Inventory.Application.Medicines.Dispense;

public record DispenseMedicineCommand(
    Guid MedicineId, 
    int Quantity
    ) : IRequest;