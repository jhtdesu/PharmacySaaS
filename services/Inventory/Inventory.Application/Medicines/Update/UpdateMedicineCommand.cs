using MediatR;

namespace Inventory.Application.Medicines.Update;

public record UpdateMedicineCommand(
    Guid Id,
    string Name,
    string SKU,
    string ActiveIngredient,
    string Unit
) : IRequest<Guid>;
