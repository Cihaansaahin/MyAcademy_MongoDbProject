using FluentValidation;
using Microsoft.Extensions.Localization;
using Travel.Web.DTOs.BannerDtos;

namespace Travel.Web.Validations.BannerValidations
{
    public class CreateBannerValidations : AbstractValidator<CreateBannerDto>
    {
        public CreateBannerValidations(IStringLocalizer<SharedResource> L)
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(_ => L["Başlık boş bırakılamaz."])
                .MinimumLength(3).WithMessage(_ => L["Başlık en az 3 karakter olmalıdır."])
                .MaximumLength(100).WithMessage(_ => L["Başlık en fazla 100 karakter olabilir."]);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => L["Açıklama boş bırakılamaz."])
                .MaximumLength(250).WithMessage(_ => L["Açıklama en fazla 250 karakter olabilir."]);

            // Yüklenen dosya "/images/banners/..." şeklinde site içi yol olur; URL ise https://... olmalı
            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage(_ => L["Görsel URL boş bırakılamaz."])
                .Must(BannerImageRules.IsValidImagePath)
                .WithMessage(_ => L["Geçerli bir görsel URL'si girin (https://...)."]);
        }
    }

    public class UpdateBannerValidator : AbstractValidator<UpdateBannerDto>
    {
        public UpdateBannerValidator(IStringLocalizer<SharedResource> L)
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(_ => L["Geçersiz istek."]);

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(_ => L["Başlık boş bırakılamaz."])
                .MinimumLength(3).WithMessage(_ => L["Başlık en az 3 karakter olmalıdır."])
                .MaximumLength(100).WithMessage(_ => L["Başlık en fazla 100 karakter olabilir."]);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage(_ => L["Açıklama boş bırakılamaz."])
                .MaximumLength(250).WithMessage(_ => L["Açıklama en fazla 250 karakter olabilir."]);

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage(_ => L["Görsel URL boş bırakılamaz."])
                .Must(BannerImageRules.IsValidImagePath)
                .WithMessage(_ => L["Geçerli bir görsel URL'si girin (https://...)."]);
        }
    }

    internal static class BannerImageRules
    {
        public static bool IsValidImagePath(string? url) =>
            !string.IsNullOrWhiteSpace(url) &&
            (url.StartsWith("/images/") || Uri.TryCreate(url, UriKind.Absolute, out _));
    }
}