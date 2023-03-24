using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Capstone.Features.PositionModule
{
	public class PositionValidator: AbstractValidator<PositionDto>
	{
		public PositionValidator() {
			RuleFor(a => a.Name)
				.NotEmpty()
				.MaximumLength(30);

			// .........
		}

		protected override void RaiseValidationException(
			ValidationContext<PositionDto> context, 
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
