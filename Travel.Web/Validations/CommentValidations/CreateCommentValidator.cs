using FluentValidation;
using Travel.Web.DTOs.CommentDtos;

namespace Travel.Web.Validations.CommentValidations
{
    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.TourId).NotEmpty().WithMessage("Tur bilgisi bulunamadı.");
            RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 yıldız arasında olmalıdır.");
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Lütfen bir yorum yazın.")
                .MinimumLength(10).WithMessage("Yorum en az 10 karakter olmalıdır.")
                .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
        }
    }
}