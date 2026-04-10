using MediatR;
using Inventory.Application.DTOs.Medicines;

namespace Inventory.Application.Medicines.Checkout;

public record CreatePendingSaleCommand(List<CheckoutItemDTO> Items) : IRequest<CreatePendingSaleResponseDTO>;
