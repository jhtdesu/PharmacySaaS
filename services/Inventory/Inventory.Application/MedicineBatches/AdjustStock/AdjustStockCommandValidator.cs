using FluentValidation;

namespace Inventory.Application.MedicineBatches.Commands.AdjustStock;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(v => v.BatchId)
            .NotEmpty().WithMessage("ID lô hàng không được để trống.");

        RuleFor(v => v.QuantityToDeduct)
            .GreaterThan(0).WithMessage("Số lượng điều chỉnh phải lớn hơn 0.");

        RuleFor(v => v.Reason)
            .NotEmpty().WithMessage("Lý do điều chỉnh không được để trống.")
            .MaximumLength(500).WithMessage("Lý do không được vượt quá 500 ký tự.");
    }
}
