using FluentValidation;
using Travel.Web.DTOs.BannerDtos;

namespace Travel.Web.Validations.BannerValidations
{
    public class CreateBannerValidations : AbstractValidator<CreateBannerDto>
    {

        public CreateBannerValidations()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Başlık Boş Bırakılamaz").MaximumLength(3).WithMessage("Başlık En Az 3 Karakter Olmalıdır.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama Boş Bırakılamaz ")
                .MaximumLength(250).WithMessage("Açıklama en fazla 250 karakter olmalıdır.");

            RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("Görsel Url Boş Bırakılamaz");

        }
    }
}
