namespace Inventory.Application.DTOs.Medicines;

public record MedicineDTO(
    Guid Id,
    string Name,
    string SKU,
    string Unit
);