using Capstone.Data;
using Capstone.ExceptionHandling;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Capstone.Features.ApplicantTracking
{
	public class ApplicantTrackingValidator: AbstractValidator<ApplicantDto>
	{
		private readonly CapstoneContext _context;

		public ApplicantTrackingValidator(CapstoneContext context) {
			_context = context;

			RuleFor(a => a.NationalId)
				.NotEmpty()
				.Must(n => new[] {9, 12}.Contains(n.Length))
				.WithMessage("Số CMND/CCCD chỉ có thể có 9 hoặc 12 số.")
				.Must(n =>
				{
					var duplicateApplicant = _context.Applicant
						.FirstOrDefault(a => a.NationalId == n);

					return duplicateApplicant == null;
				})
				.WithMessage("Số CMND/CCCD không được trùng.");

			RuleFor(a => a.FullName)
				.NotEmpty()
				.MaximumLength(30);

			RuleFor(a => a.Gender)
				.NotEmpty()
				.Must(g => new[] { "male", "female", "other" }.Contains(g));

			RuleFor(a => a.BirthDate)
				.InclusiveBetween(
					from: DateTime.Now.AddYears(-60),
					to: DateTime.Now.AddYears(-18)
				);

			RuleFor(a => a.Address)
				.NotEmpty()
				.MaximumLength(200);

			RuleFor(a => a.Phone)
				.NotEmpty()
				.MinimumLength(10)
				.MaximumLength(11);

			RuleFor(a => a.Email)
				.EmailAddress()
				.When(a => a.Email != string.Empty);

			RuleFor(a => a.ExperienceYears)
				.NotEmpty()
				.GreaterThanOrEqualTo(0);

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
