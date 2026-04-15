using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Sales.GetById;

public record GetSaleByIdQuery(Guid Id) : IRequest<SaleDetailsDTO?>;

public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDetailsDTO?>
{
    private readonly IInventoryDbContext _context;

    public GetSaleByIdQueryHandler(IInventoryDbContext context) => _context = context;

    public async Task<SaleDetailsDTO?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales.AsNoTracking().Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (sale == null) return null;

        var items = sale.Items.Select(i => new SaleItemDTO(i.MedicineId, i.Quantity, i.UnitPrice, i.SubTotal)).ToList();

        return new SaleDetailsDTO(sale.Id, sale.ReceiptNumber, sale.SaleDate, sale.TotalAmount, sale.ProcessedBy, items);
    }
}