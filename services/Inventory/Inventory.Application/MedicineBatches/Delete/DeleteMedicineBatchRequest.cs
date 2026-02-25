using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;

public record DeleteMedicineBatchRequest(Guid Id) : IRequest<bool>;

public class DeleteMedicineBatchHandler : IRequestHandler<DeleteMedicineBatchRequest, bool>
{
    private readonly IInventoryDbContext _context;
    public DeleteMedicineBatchHandler(IInventoryDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteMedicineBatchRequest request, CancellationToken ct)
    {
        var medicineBatch = await _context.Batches.Where(mb => mb.Id == request.Id).FirstOrDefaultAsync(ct);

        if (medicineBatch == null) return false;

        medicineBatch.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}