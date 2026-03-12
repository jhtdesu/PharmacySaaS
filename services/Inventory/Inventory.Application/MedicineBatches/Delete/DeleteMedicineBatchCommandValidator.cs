using FluentValidation;

namespace Inventory.Application.MedicineBatches.Delete;

public class DeleteMedicineBatchCommandValidator : AbstractValidator<DeleteMedicineBatchCommand>
{
    public DeleteMedicineBatchCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");
    }
}
