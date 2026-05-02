namespace Inventory.Application.DTOs.Medicines;

public record UpdateMedicineRequestDTO(
    string Name,
    string SKU,
    string ActiveIngredient,
    string Unit,
    string ImageUrl
);