namespace Inventory.Application.DTOs.MedicineBatches;

public record UpdateMedicineBatchRequestDTO(
    string BatchNumber,
    DateTime ExpiryDate,
    int Quantity
);