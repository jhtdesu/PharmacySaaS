using MediatR;

namespace Inventory.Application.Medicines.Checkout;

public record CompleteSaleCommand(Guid SaleId) : IRequest<string>;
