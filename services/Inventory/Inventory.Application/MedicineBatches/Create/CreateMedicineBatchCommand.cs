using MediatR;

namespace Inventory.Application.MedicineBatches.Create;

public record CreateMedicineBatchCommand(
    Guid MedicineId,     
    string BatchNumber,  
    DateTime ExpiryDate,  
    int Quantity         
) : IRequest<Guid>;