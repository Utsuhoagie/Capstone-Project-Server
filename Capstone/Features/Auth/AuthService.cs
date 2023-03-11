using Capstone.Data;
using Capstone.Features.Auth.Models;
using Capstone.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Capstone.Features.Auth
{
    public class AuthService: IAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly CapstoneContext _context;
		private readonly UserManager<EmployeeUser> _userManager;
		private readonly IValidator<RegisterRequest> _registerValidator;

		public AuthService(
			IConfiguration configuration,
			CapstoneContext context, 
			UserManager<EmployeeUser> userManager,
			IValidator<RegisterRequest> registerValidator) 
		{
			_configuration = configuration;
			_context = context;
			_userManager = userManager;
			_registerValidator = registerValidator;
		}
		
		public async Task<AuthResponse> Login(LoginRequest req)
		{
			var existingUser = await _userManager.FindByEmailAsync(req.Email);

			if (existingUser == null)
			{
				return new AuthResponse { Status = HttpStatusCode.Unauthorized };
			}

			var isUserValid = await _userManager.CheckPasswordAsync(existingUser, req.Password);

			if (!isUserValid)
			{
				return new AuthResponse { Status = HttpStatusCode.Unauthorized };
			}

			var userRoles = await _userManager.GetRolesAsync(existingUser);

			var secretKey =
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:SecretKey").Value));

			var signingCredentials =
				new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Email, existingUser.Email),
				new Claim(ClaimTypes.Role, userRoles.Single()),
			};

			var token = new JwtSecurityToken(
				issuer: "https://localhost:5000",
				audience: "https://localhost:3000",
				expires: DateTime.Now.AddMinutes(10),
				signingCredentials: signingCredentials,
				claims: claims
			);

			var serializedToken = new JwtSecurityTokenHandler()
				.WriteToken(token);

			return new AuthResponse
			{
				Status = HttpStatusCode.OK,
				Token = serializedToken
			};
		}

		/*public async Task<AuthResponse> Register(RegisterRequest req)
{
	//await _registerValidator.ValidateAndThrowAsync(req);

	var user = await _userManager.FindByEmailAsync(req.Email);

	if (user != null || (req.Password != req.PasswordConfirm))
	{
		return new AuthResponse
		{
			Status = HttpStatusCode.BadRequest
		};
	}

	user = new EmployeeUser
	{
		UserName = req.Email,
		Email = req.Email
	};

	var result = await _userManager.CreateAsync(user, req.Password);

	if (!result.Succeeded)
	{
		return new AuthResponse
		{
			Status = HttpStatusCode.BadRequest,
			Errors = result.Errors,
		};
	}

	await _userManager.AddToRoleAsync(user, AuthRoles.Admin);

	return new AuthResponse
	{
		Status = HttpStatusCode.Created
		//Token = ...
	};
}
*/


		public async Task<bool> DEBUG_DELETE(string email)
		{
			var existingUser = await _userManager.FindByEmailAsync(email);

			if (existingUser == null)
			{
				return false;
			}

			await _userManager.DeleteAsync(existingUser);

			return true;
		}
	}
}

