using FluentValidation;
using Travel.Web.DTOs.TourDtos;

namespace Travel.Web.Validations.TourValidations
{
    public class CreateTourValidator : AbstractValidator<CreateTourDto>
    {
        public CreateTourValidator()
        {
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

            // Kapak görseli: URL veya dosya, en az biri
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl) || x.CoverImageFile != null)
                .WithName("CoverImageUrl")
                .WithMessage("Kapak görseli yükleyin veya bir görsel URL'si girin.");

            // Nested: Tur tarihleri
            RuleFor(x => x.TourDates).NotEmpty().WithMessage("En az bir tur tarihi eklemelisiniz.");
            RuleForEach(x => x.TourDates).SetValidator(new TourDateItemValidator());
            RuleForEach(x => x.TourDates)
                .Must(d => d.StartDate.Date >= DateTime.Today)
                .WithMessage("Yeni tur tarihleri geçmiş bir tarih olamaz.");

            // Nested: Gün gün program
            RuleForEach(x => x.Itinerary).SetValidator(new TourPlanDayValidator());
            RuleFor(x => x.Itinerary.Count)
                .LessThanOrEqualTo(x => x.DurationDays)
                .WithMessage("Program gün sayısı tur süresinden fazla olamaz.");
        }
    }
}