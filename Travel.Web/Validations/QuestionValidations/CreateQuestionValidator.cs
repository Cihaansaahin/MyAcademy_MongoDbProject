using FluentValidation;
using Travel.Web.DTOs.QuestionDtos;

namespace Travel.Web.Validations.QuestionValidations
{
    public class CreateQuestionValidator : AbstractValidator<CreateQuestionDto>
    {
        public CreateQuestionValidator()
        {
            RuleFor(x => x.TourId).NotEmpty().WithMessage("Tur bilgisi bulunamadı.");
            RuleFor(x => x.QuestionText)
                .NotEmpty().WithMessage("Lütfen sorunuzu yazın.")
                .MinimumLength(10).WithMessage("Soru en az 10 karakter olmalıdır.")
                .MaximumLength(500).WithMessage("Soru en fazla 500 karakter olabilir.");
        }
    }
}