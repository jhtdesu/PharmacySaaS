using FluentValidation;

namespace Inventory.Application.Medicines.Delete;

public class DeleteMedicineCommandValidator : AbstractValidator<DeleteMedicineCommand>
{
    public DeleteMedicineCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("ID thuốc không được để trống.");
    }
}
