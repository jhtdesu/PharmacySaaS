using MediatR;
using Shared.Contracts.Models;
using Inventory.Application.DTOs.MedicineBatches;

namespace Inventory.Application.MedicineBatches.Queries.GetExpiringBatches;

public record GetExpiringBatchesQuery(int DaysThreshold = 90, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<ExpiringBatchDTO>>>;