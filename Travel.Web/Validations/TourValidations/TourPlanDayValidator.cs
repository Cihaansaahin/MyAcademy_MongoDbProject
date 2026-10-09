using FluentValidation;
using Travel.Web.Entities;

namespace Travel.Web.Validations.TourValidations
{
    public class TourPlanDayValidator : AbstractValidator<TourPlanDay>
    {
        public TourPlanDayValidator()
        {
            RuleFor(x => x.DayNumber)
                .GreaterThan(0).WithMessage("Gün numarası 1 veya daha büyük olmalıdır.");

            RuleFor(x => x.Title.Tr)
                .NotEmpty().WithMessage("Her program gününün bir başlığı olmalıdır.")
                .MaximumLength(150).WithMessage("Gün başlığı en fazla 150 karakter olabilir.");
        }
    }
}