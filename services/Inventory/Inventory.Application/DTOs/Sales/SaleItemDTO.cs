namespace Inventory.Application.DTOs.Sales;
public record SaleItemDTO(
    Guid MedicineId, 
    int Quantity, 
    decimal UnitPrice, 
    decimal SubTotal
);