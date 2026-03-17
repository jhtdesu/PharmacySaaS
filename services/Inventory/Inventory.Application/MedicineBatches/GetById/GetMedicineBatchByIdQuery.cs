using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.MedicineBatches;

namespace Inventory.Application.MedicineBatches.GetById;

public record GetMedicineBatchByIdQuery(Guid Id) : IRequest<MedicineBatchDTO?>;

public class GetMedicineBatchByIdQueryHandler : IRequestHandler<GetMedicineBatchByIdQuery, MedicineBatchDTO?>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineBatchByIdQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<MedicineBatchDTO?> Handle(GetMedicineBatchByIdQuery request, CancellationToken ct)
    {
        return await _context.Batches
            .Where(mb => mb.Id == request.Id && !mb.IsDeleted)
            .Select(mb => new MedicineBatchDTO(mb.Id, mb.BatchNumber, mb.ExpiryDate, mb.OriginalQuantity))
            .FirstOrDefaultAsync(ct);
    }
}