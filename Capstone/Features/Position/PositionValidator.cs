using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using FluentValidation;
using FluentValidation.Results;
using Capstone.Features.PositionModule.Models;

namespace Capstone.Features.PositionModule
{
    public class PositionValidator: AbstractValidator<PositionRequest>
	{
		public PositionValidator() {
			RuleFor(a => a.Name)
				.NotEmpty()
				.MaximumLength(30);

			// .........
		}

		protected override void RaiseValidationException(
			ValidationContext<PositionRequest> context, 
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
