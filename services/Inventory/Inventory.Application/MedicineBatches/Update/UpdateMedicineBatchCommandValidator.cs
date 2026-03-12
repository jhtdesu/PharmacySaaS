using FluentValidation;

namespace Inventory.Application.MedicineBatches.Update;

public class UpdateMedicineBatchCommandValidator : AbstractValidator<UpdateMedicineBatchCommand>
{
    public UpdateMedicineBatchCommandValidator()
    {
        RuleFor(v => v.BatchId)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");

        RuleFor(v => v.BatchNumber)
            .NotEmpty().WithMessage("Số lô không được để trống.")
            .MaximumLength(100).WithMessage("Số lô không được vượt quá 100 ký tự.");

        RuleFor(v => v.ExpiryDate)
            .NotEmpty().WithMessage("Ngày hết hạn không được để trống.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Ngày hết hạn phải lớn hơn ngày hiện tại.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");
    }
}
