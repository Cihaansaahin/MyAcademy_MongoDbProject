using FluentValidation;
using Travel.Web.Entities;

namespace Travel.Web.Validations.TourValidations
{
    public class TourDateItemValidator : AbstractValidator<TourDateItem>
    {
        public TourDateItemValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Tur başlangıç tarihi boş bırakılamaz.");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.");

            RuleFor(x => x.Capacity)
                .InclusiveBetween(1, 500).WithMessage("Kontenjan 1 ile 500 arasında olmalıdır.");
        }
    }
}