using MediatR;

namespace Inventory.Application.MedicineBatches.Update;

public record UpdateMedicineBatchCommand(
    Guid BatchId,
    string BatchNumber,  
    DateTime ExpiryDate,  
    int Quantity         
) : IRequest<Guid>;