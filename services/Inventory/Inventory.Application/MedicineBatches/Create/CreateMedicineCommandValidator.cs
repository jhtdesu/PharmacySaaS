using FluentValidation;

namespace Inventory.Application.Medicines.Create;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("Tên thuốc không được để trống.").MaximumLength(200).WithMessage("Tên thuốc không được vượt quá 200 ký tự.");

        RuleFor(v => v.SKU).NotEmpty().WithMessage("SKU không được để trống.");

        RuleFor(v => v.Unit).NotEmpty().WithMessage("Đơn vị tính không được để trống (VD: Viên, Vỉ, Hộp).");
    }
}