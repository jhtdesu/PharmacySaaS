namespace Inventory.Application.DTOs.Sales;
public record SaleDetailsDTO(
    Guid Id, 
    string ReceiptNumber, 
    DateTime SaleDate, 
    decimal TotalAmount, 
    Guid ProcessedBy, 
    List<SaleItemDTO> Items
);