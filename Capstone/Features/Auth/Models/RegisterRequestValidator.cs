using Capstone.Responses.ExceptionHandling;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Capstone.Features.Auth.Models
{
	public class RegisterRequestValidator: AbstractValidator<RegisterRequest>
	{
		public RegisterRequestValidator() 
		{
			RuleFor(req => req.Email)
				.EmailAddress()
				.WithMessage("Email không hợp lệ.")
				.NotEmpty()
				.WithMessage("Email không được trống.");

			RuleFor(req => req.Password)
				.NotEmpty()
				.WithMessage("Mật khẩu không được trống.");

			RuleFor(req => req.PasswordConfirm)
				.Must((req, PasswordConfirm) => req.PasswordConfirm == PasswordConfirm)
				.WithMessage("Mật khẩu không trùng khớp.")
				.NotEmpty()
				.WithMessage("Mật khẩu nhập lại không được trống.");
		}

		protected override void RaiseValidationException(
			ValidationContext<RegisterRequest> context,
			ValidationResult result)
		{
			throw new CustomValidationException(result.Errors);
		}
	}
}
