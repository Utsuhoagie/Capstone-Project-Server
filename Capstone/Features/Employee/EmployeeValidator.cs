﻿using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using FluentValidation;
using FluentValidation.Results;
using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.EmployeeModule
{
    public class EmployeeValidator: AbstractValidator<EmployeeRequest>
	{
		public EmployeeValidator() {
			RuleFor(e => e.NationalId)
				.NotEmpty()
				.Must(n => new[] { 9, 12 }.Contains(n.Length))
				.WithMessage("Số CMND/CCCD chỉ có thể có 9 hoặc 12 số.");
				//.Must(n =>
				//{
				//	var duplicateEmployee = _context.People
				//		.FirstOrDefault(e => e.NationalId == n);

				//	return duplicateEmployee == null;
				//})
				//.WithMessage("Số CMND/CCCD không được trùng.");

			RuleFor(e => e.FullName)
				.NotEmpty()
				.MaximumLength(30);

			RuleFor(e => e.Gender)
				.NotEmpty()
				.Must(g => new[] { "male", "female", "other" }.Contains(g));

			RuleFor(e => e.BirthDate)
				.InclusiveBetween(
					from: DateTime.Now.AddYears(-60),
					to: DateTime.Now.AddYears(-18)
				);

			RuleFor(e => e.Address)
				.NotEmpty()
				.MaximumLength(200);

			RuleFor(e => e.Phone)
				.NotEmpty()
				.MinimumLength(10)
				.MaximumLength(11);

			RuleFor(e => e.Email)
				.EmailAddress()
				.When(e => e.Email != string.Empty);

			RuleFor(e => e.ExperienceYears)
				.NotNull()
				.GreaterThanOrEqualTo(0);

			RuleFor(e => e.PositionName)
				.NotEmpty();

			RuleFor(e => e.Salary)
				.NotNull()
				.GreaterThanOrEqualTo(1_000_000);

			RuleFor(e => e.EmployedDate)
				.NotEmpty()
				.LessThanOrEqualTo(DateTime.Now);
		}

		protected override void RaiseValidationException(
			ValidationContext<EmployeeRequest> context, 
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
