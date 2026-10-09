using FluentValidation;
using Travel.Web.DTOs.QuestionDtos;

namespace Travel.Web.Validations.QuestionValidations
{
    public class AnswerQuestionValidator : AbstractValidator<AnswerQuestionDto>
    {
        public AnswerQuestionValidator()
        {
            RuleFor(x => x.QuestionId).NotEmpty().WithMessage("Soru bulunamadı.");
            RuleFor(x => x.AnswerText)
                .NotEmpty().WithMessage("Cevap boş bırakılamaz.")
                .MinimumLength(5).WithMessage("Cevap en az 5 karakter olmalıdır.")
                .MaximumLength(1000).WithMessage("Cevap en fazla 1000 karakter olabilir.");
        }
    }
}