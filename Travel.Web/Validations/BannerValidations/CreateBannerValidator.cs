using FluentValidation;
using Travel.Web.DTOs.BannerDtos;

namespace Travel.Web.Validations.BannerValidations
{
    public class CreateBannerValidations : AbstractValidator<CreateBannerDto>
    {
        public CreateBannerValidations()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Başlık en az 3 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Başlık en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama boş bırakılamaz.")
                .MaximumLength(250).WithMessage("Açıklama en fazla 250 karakter olabilir.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Görsel URL boş bırakılamaz.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir görsel URL'si girin (https://...).");
        }
    }
}