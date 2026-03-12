using FluentValidation;

namespace Inventory.Application.Medicines.Update;

public class UpdateMedicineCommandValidator : AbstractValidator<UpdateMedicineCommand>
{
    public UpdateMedicineCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID thuốc không được để trống.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên thuốc không được để trống.")
            .MaximumLength(200).WithMessage("Tên thuốc không được vượt quá 200 ký tự.");

        RuleFor(v => v.SKU)
            .NotEmpty().WithMessage("SKU không được để trống.")
            .MaximumLength(50).WithMessage("SKU không được vượt quá 50 ký tự.");

        RuleFor(v => v.ActiveIngredient)
            .NotEmpty().WithMessage("Hoạt chất không được để trống.");

        RuleFor(v => v.Unit)
            .NotEmpty().WithMessage("Đơn vị tính không được để trống (VD: Viên, Vỉ, Hộp).");
    }
}
