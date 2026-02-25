namespace Inventory.Application.DTOs.MedicineBatches;

public record MedicineBatchDTO(
    Guid Id,
    string BatchNumber,
    DateTime ExpiryDate,
    int Quantity
);