using MediatR;

namespace Inventory.Application.Medicines.Checkout;

public record CheckoutCommand(List<CheckoutItemDTO> Items) : IRequest<string>;