using MediatR;
using Shared.Contracts.Models;

namespace Inventory.Application.MedicineBatches.Commands.AdjustStock;

public record AdjustStockCommand(
    Guid BatchId, 
    int QuantityToDeduct, 
    string Reason) : IRequest<BaseResponse<bool>>;