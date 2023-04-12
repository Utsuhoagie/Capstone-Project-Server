using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Capstone.Features.AttendanceModule
{
	public class AttendanceValidator: AbstractValidator<AttendanceDto>
	{
		public AttendanceValidator() {
			/*RuleFor(a => a.NationalId)
				.NotEmpty()
				.Must(n => new[] { 9, 12 }.Contains(n.Length))
				.WithMessage("Số CMND/CCCD chỉ có thể có 9 hoặc 12 số.");
			//.Must(n =>
			//{
			//	var duplicateApplicant = _context.People.OfType<Applicant>()
			//		.FirstOrDefault(a => a.NationalId == n);

			//	return duplicateApplicant == null;
			//})
			//.WithMessage("Số CMND/CCCD không được trùng.");

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
				.NotNull()
				.GreaterThanOrEqualTo(0);

			RuleFor(a => a.AppliedPositionName)
				.NotEmpty();

			RuleFor(a => a.AppliedDate)
				.NotEmpty()
				.LessThanOrEqualTo(DateTime.Now);

			RuleFor(a => a.AskingSalary)
				.NotNull()
				.GreaterThanOrEqualTo(1_000_000);*/
		}

		protected override void RaiseValidationException(
			ValidationContext<AttendanceDto> context, 
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
