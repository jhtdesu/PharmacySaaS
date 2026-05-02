using MediatR;

namespace Inventory.Application.Medicines.Create;

public record CreateMedicineCommand(
    string Name,
    string SKU,
    string ActiveIngredient,
    string Unit,
    string ImageUrl) : IRequest<Guid>;