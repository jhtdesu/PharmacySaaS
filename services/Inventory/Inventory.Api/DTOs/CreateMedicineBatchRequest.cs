namespace Inventory.Api.DTOs;

public record CreateMedicineBatchRequest(
    string BatchNumber,
    DateTime ExpiryDate,
    int Quantity
);