using Capstone.Responses.ExceptionHandling;
using FluentValidation.Results;

namespace Capstone.Responses.ExceptionHandling
{
    public class CustomValidationException : Exception
    {
        public CustomValidationException(List<ValidationFailure> errors)
        {
            var formattedErrors = errors
                .GroupBy(e => e.PropertyName)
                .Select(gr => gr
                    .Aggregate(
                        seed: new CustomValidationFailure { PropertyName = gr.Key },
                        (propertyGroup, next) =>
                        {
                            propertyGroup.Errors.Add(next.ErrorMessage);
                            return propertyGroup;
                        }
                    )
                );

            throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest, formattedErrors);
        }
    }
}
