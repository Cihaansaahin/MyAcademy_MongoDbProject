using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Travel.Web.Validations
{
    public static class ValidationExtensions
    {
        // FluentValidation hatalarını MVC'nin ModelState'ine aktarır
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
        {
            foreach (var error in result.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        // AJAX (JSON) cevapları için tüm hataları tek mesajda birleştirir
        public static string ToMessage(this ValidationResult result)
            => string.Join("\n", result.Errors.Select(e => e.ErrorMessage).Distinct());
    }
}
