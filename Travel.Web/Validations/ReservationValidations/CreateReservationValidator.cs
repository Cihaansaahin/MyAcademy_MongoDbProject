using FluentValidation;
using Travel.Web.DTOs.ReservationDtos;

namespace Travel.Web.Validations.ReservationValidations
{
    public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationValidator()
        {
            RuleFor(x => x.TourId).NotEmpty().WithMessage("Tur bilgisi bulunamadı.");
            RuleFor(x => x.TourDateId).NotEmpty().WithMessage("Lütfen bir tur tarihi seçin.");
            RuleFor(x => x.AdultCount).InclusiveBetween(1, 20).WithMessage("Yetişkin sayısı 1 ile 20 arasında olmalıdır.");
            RuleFor(x => x.ChildCount).InclusiveBetween(0, 20).WithMessage("Çocuk sayısı 0 ile 20 arasında olmalıdır.");
            RuleFor(x => x.AdultCount + x.ChildCount)
                .LessThanOrEqualTo(30).WithName("Toplam")
                .WithMessage("Tek seferde en fazla 30 kişilik rezervasyon yapılabilir.");
        }
    }
}