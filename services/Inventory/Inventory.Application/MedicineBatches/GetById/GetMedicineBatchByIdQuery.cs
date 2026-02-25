using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.MedicineBatches;

public record GetMedicineBatchByIdQuery(Guid Id) : IRequest<MedicineBatchDTO?>;

public class GetMedicineBatchByIdHandler : IRequestHandler<GetMedicineBatchByIdQuery, MedicineBatchDTO?>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineBatchByIdHandler(IInventoryDbContext context) => _context = context;

    public async Task<MedicineBatchDTO?> Handle(GetMedicineBatchByIdQuery request, CancellationToken ct)
    {
        return await _context.Batches
            .Where(mb => mb.Id == request.Id)
            .Select(mb => new MedicineBatchDTO(mb.Id, mb.BatchNumber, mb.ExpiryDate, mb.OriginalQuantity))
            .FirstOrDefaultAsync(ct);
    }
}