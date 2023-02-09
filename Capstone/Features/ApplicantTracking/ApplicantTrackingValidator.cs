using Capstone.ExceptionHandling;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Capstone.Features.ApplicantTracking
{
	public class ApplicantTrackingValidator: AbstractValidator<ApplicantDto>
	{
		private readonly int[] NationalIdLengths = new [] { 9, 12 };
		private readonly string[] GenderOptions = new [] { "male", "female", "other" };

		public ApplicantTrackingValidator() {
			RuleFor(a => a.NationalId)
				.NotEmpty()
				.Must(n => NationalIdLengths.Contains(n.Length))
				.WithMessage("Số CMND/CCCD chỉ có thể có 9 hoặc 12 số.");

			RuleFor(a => a.FullName)
				.NotEmpty()
				.MaximumLength(30);

			RuleFor(a => a.Gender)
				.NotEmpty()
				.Must(g => GenderOptions.Contains(g));

			RuleFor(a => a.BirthDate)
				.InclusiveBetween(
					from: DateTime.Now.AddYears(-60),
					to: DateTime.Now.AddYears(-18)
				);

			RuleFor(a => a.Address)
				.MaximumLength(200);

			RuleFor(a => a.Phone)
				.NotEmpty()
				.MinimumLength(10)
				.MaximumLength(11);

			RuleFor(a => a.Email)
				.EmailAddress();

			RuleFor(a => a.AppliedPosition)
				.NotEmpty();

			RuleFor(a => a.AppliedDate)
				.NotEmpty()
				.LessThanOrEqualTo(DateTime.Now);

			RuleFor(a => a.AskingSalary)
				.NotEmpty()
				.GreaterThanOrEqualTo(1_000_000);
		}

		protected override void RaiseValidationException(
			ValidationContext<ApplicantDto> context, 
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
