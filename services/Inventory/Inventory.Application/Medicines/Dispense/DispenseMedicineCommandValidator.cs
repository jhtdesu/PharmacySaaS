using FluentValidation;

namespace Inventory.Application.Medicines.Dispense;

public class DispenseMedicineCommandValidator : AbstractValidator<DispenseMedicineCommand>
{
    public DispenseMedicineCommandValidator()
    {
        RuleFor(v => v.MedicineId)
            .NotEmpty().WithMessage("ID thuốc không được để trống.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Số lượng xuất phải lớn hơn 0.");
    }
}
