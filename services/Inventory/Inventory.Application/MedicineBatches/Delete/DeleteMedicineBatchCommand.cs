using MediatR;
using Microsoft.EntityFrameworkCore;
using Inventory.Application.Common.Interfaces;

namespace Inventory.Application.MedicineBatches.Delete;

public record DeleteMedicineBatchCommand(Guid Id) : IRequest<bool>;

public class DeleteMedicineBatchCommandHandler : IRequestHandler<DeleteMedicineBatchCommand, bool>
{
    private readonly IInventoryDbContext _context;
    public DeleteMedicineBatchCommandHandler(IInventoryDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteMedicineBatchCommand request, CancellationToken ct)
    {
        var medicineBatch = await _context.Batches.Where(mb => mb.Id == request.Id).FirstOrDefaultAsync(ct);

        if (medicineBatch == null) return false;

        medicineBatch.IsDeleted = true;
        await _context.SaveChangesAsync(ct);

        return true;
    }
}