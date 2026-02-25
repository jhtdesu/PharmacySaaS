using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.MedicineBatches;

public record GetMedicineBatchesQuery() : IRequest<List<MedicineBatchDTO>>;

public class GetMedicineBatchesHandler : IRequestHandler<GetMedicineBatchesQuery, List<MedicineBatchDTO>>
{
    private readonly IInventoryDbContext _context;
    public GetMedicineBatchesHandler(IInventoryDbContext context) => _context = context;

    public async Task<List<MedicineBatchDTO>> Handle(GetMedicineBatchesQuery request, CancellationToken ct)
    {
        return await _context.Batches.Select(b => new MedicineBatchDTO(b.Id, b.BatchNumber, b.ExpiryDate, b.OriginalQuantity)).ToListAsync(ct);
    }
}