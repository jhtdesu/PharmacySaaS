using Inventory.Application.DTOs.Medicines;
using MediatR;
using Shared.Contracts.Models;

namespace Inventory.Application.Medicines.Queries.GetLowStock;

public record GetLowStockMedicinesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResponse<List<LowStockMedicineDTO>>>;