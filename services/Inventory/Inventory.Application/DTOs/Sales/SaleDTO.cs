namespace Inventory.Application.DTOs.Sales;
public record SaleDTO(
    Guid Id, 
    string ReceiptNumber, 
    DateTime SaleDate, 
    decimal TotalAmount, 
    Guid ProcessedByUserId
);