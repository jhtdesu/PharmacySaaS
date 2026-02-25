namespace Inventory.Application.DTOs.MedicineBatches;

public record CreateMedicineBatchRequest(
    string BatchNumber,
    DateTime ExpiryDate,
    int Quantity
);