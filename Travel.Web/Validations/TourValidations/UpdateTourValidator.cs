using FluentValidation;
using Travel.Web.DTOs.TourDtos;

namespace Travel.Web.Validations.TourValidations
{
    public class UpdateTourValidator : AbstractValidator<UpdateTourDto>
    {
        public UpdateTourValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Güncellenecek tur bulunamadı.");

            // Çok dilli alanlar
            RuleFor(x => x.Title.Tr).NotEmpty().WithMessage("Tur adı (Türkçe) boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Tur adı en az 3 karakter olmalıdır.")
                .MaximumLength(150).WithMessage("Tur adı en fazla 150 karakter olabilir.");
            RuleFor(x => x.Title.En).NotEmpty().WithMessage("Tur adı (İngilizce) boş bırakılamaz.");
            RuleFor(x => x.Description.Tr).NotEmpty().WithMessage("Açıklama (Türkçe) boş bırakılamaz.");
            RuleFor(x => x.Description.En).NotEmpty().WithMessage("Açıklama (İngilizce) boş bırakılamaz.");

            // Temel bilgiler
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");
            RuleFor(x => x.Country).NotEmpty().WithMessage("Ülke boş bırakılamaz.");
            RuleFor(x => x.City).NotEmpty().WithMessage("Şehir boş bırakılamaz.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Lütfen bir kategori seçin.");
            RuleFor(x => x.DestinationId).NotEmpty().WithMessage("Lütfen bir destinasyon seçin.");
            RuleFor(x => x.DurationDays).InclusiveBetween(1, 60).WithMessage("Tur süresi 1 ile 60 gün arasında olmalıdır.");

            // Nested: Tur tarihleri (güncellemede geçmiş tarihlere izin var)
            RuleFor(x => x.TourDates).NotEmpty().WithMessage("En az bir tur tarihi olmalıdır.");
            RuleForEach(x => x.TourDates).SetValidator(new TourDateItemValidator());

            // Nested: Gün gün program
            RuleForEach(x => x.Itinerary).SetValidator(new TourPlanDayValidator());
            RuleFor(x => x.Itinerary.Count)
                .LessThanOrEqualTo(x => x.DurationDays)
                .WithMessage("Program gün sayısı tur süresinden fazla olamaz.");
        }
    }
}