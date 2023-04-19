using Capstone.Data;
using Capstone.Features.Auth.Models;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Capstone.Features.Auth
{
    public interface IAuthService
	{
		string GenerateAccessToken(EmployeeUser user, Employee? employee, string userRole);
		string GenerateRefreshToken();
		Task<AuthResponse> RegisterEmployee(RegisterEmployeeRequest req);
		Task<AuthResponse> Login(LoginRequest req);
		Task<AuthResponse> Refresh(RefreshRequest req);
		Task<AuthResponse> DEBUG_REGISTER(RegisterRequest req);
		Task<ServiceResult> DEBUG_DELETE(string email);
	}

	public class AuthService : IAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly CapstoneContext _context;
		private readonly UserManager<EmployeeUser> _userManager;
		private readonly IValidator<RegisterRequest> _registerValidator;

		private readonly int ACCESS_EXPIRY_MINUTES = 180;
		private readonly int REFRESH_EXPIRY_MINUTES = 900;

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

		#region===== Auth internal methods ======
		public string GenerateAccessToken(EmployeeUser user, Employee? employee, string userRole)
		{
			var secretKey =
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:SecretKey").Value));

			var signingCredentials =
				new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);


			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Email),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, userRole),

				// these claims, including the added one below this block
				// are not really used for BE,
				// for easier FE usage
				// for route params, or for checking on auth screen, etc
				new Claim("Email", user.Email),
				new Claim("Role", userRole)
			};

			if (employee != null)
			{
				claims.Add(new Claim("NationalId", employee.NationalId));
			}

			var accessSecurityToken = new JwtSecurityToken(
				issuer: "https://localhost:5000",
				audience: "https://localhost:3000",
				expires: DateTime.Now.AddMinutes(ACCESS_EXPIRY_MINUTES),
				signingCredentials: signingCredentials,
				claims: claims
			);

			var accessToken = new JwtSecurityTokenHandler()
				.WriteToken(accessSecurityToken);

			return accessToken;
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];

			var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);

			return Convert.ToBase64String(randomNumber);
		}

		public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateIssuerSigningKey = true,
				ValidateLifetime = false,
				ClockSkew = TimeSpan.Zero,
				ValidIssuer = _configuration.GetSection("JWT:ValidIssuer").Value,
				ValidAudiences = _configuration.GetSection("JWT:ValidAudiences").Get<string[]>(),
				IssuerSigningKey = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(
						_configuration.GetSection("JWT:SecretKey").Value
					))
			};

			var handler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;
			var principal = handler.ValidateToken(
				accessToken,
				tokenValidationParameters,
				out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;

			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new SecurityTokenException("Invalid accessToken");
			}

			return principal;
		}

		public bool IsRefreshValid(RefreshRequest req, EmployeeUser user)
		{
			var accessToken = req.AccessToken;
			var refreshToken = req.RefreshToken;

			var isUserNull = user == null;
			var isRefreshTokenValid = refreshToken == user?.RefreshToken;
			var isRefreshTokenExpired = user?.RefreshTokenExpiryTime <= DateTime.Now;

			if (isUserNull || !isRefreshTokenValid || isRefreshTokenExpired)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		#endregion

		#region===== Auth services =======
		public async Task<AuthResponse> RegisterEmployee(RegisterEmployeeRequest req)
		{
			//await _registerValidator.ValidateAndThrowAsync(req);

			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.Position)
				.SingleOrDefaultAsync(e => e.Email == req.Email);

			var duplicateEmployeeUser = await _userManager.FindByEmailAsync(req.Email);

			if (employee == null)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.NotFound,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.NoEmployeeError,
						}
					}
				};
			}

			if (duplicateEmployeeUser != null)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.BadRequest,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.DuplicateEmployeeUserError,
						}
					}
				};
			}

			if (req.Password != req.PasswordConfirm)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.BadRequest,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.PasswordsDontMatchError,
						},
					}
				};
			}

			var newEmployeeUser = new EmployeeUser
			{
				UserName = req.Email,
				Email = req.Email,
				Employee = employee
			};

			var result = await _userManager.CreateAsync(newEmployeeUser, req.Password);

			if (!result.Succeeded)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.BadRequest,
					Errors = result.Errors,
				};
			}

			var employeePositionName = employee.Position.Name;
			var employeeUserRole =
				employeePositionName == "Nhân sự" ? AuthRoles.Admin : AuthRoles.Employee;

			await _userManager.AddToRoleAsync(newEmployeeUser, employeeUserRole);

			return new AuthResponse
			{
				Status = HttpStatusCode.Created
				//AccessToken = ...
			};
		}
		public async Task<AuthResponse> Login(LoginRequest req)
		{
			var existingUser = await _userManager.FindByEmailAsync(req.Email);

			if (existingUser == null)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.NotFound,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.NoEmployeeUserError,
						}
					}
				};
			}

			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.FirstOrDefaultAsync(e => e.User == existingUser);

			if (employee == null && existingUser.Email != "master@example.com")
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.NotFound,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.NoEmployeeError,
						}
					}
				};
			}

			var isUserValid = await _userManager.CheckPasswordAsync(existingUser, req.Password);

			if (!isUserValid)
			{
				return new AuthResponse { Status = HttpStatusCode.Unauthorized };
			}

			var userRoles = await _userManager.GetRolesAsync(existingUser);
			var userRole = userRoles.SingleOrDefault()!;

			var accessToken = GenerateAccessToken(existingUser, employee, userRole);
			var refreshToken = GenerateRefreshToken();



			existingUser.RefreshToken = refreshToken;
			existingUser.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(REFRESH_EXPIRY_MINUTES);

			await _userManager.UpdateAsync(existingUser);

			return new AuthResponse
			{
				Status = HttpStatusCode.OK,
				AccessToken = accessToken,
				RefreshToken = refreshToken,
			};
		}

		public async Task<AuthResponse> Refresh(RefreshRequest req)
		{
			var accessToken = req.AccessToken;
			var refreshToken = req.RefreshToken;

			var principal = GetPrincipalFromExpiredToken(accessToken);
			var userEmail = (principal.Identity)?.Name;

			var user = await _userManager.FindByEmailAsync(userEmail);
			var userRole = (await _userManager.GetRolesAsync(user)).SingleOrDefault();

			var isUserNull = user == null;
			var isRefreshTokenValid = refreshToken == user?.RefreshToken;
			var isRefreshTokenExpired = user?.RefreshTokenExpiryTime <= DateTime.Now;

			if (!IsRefreshValid(req, user) || userRole == null)
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.BadRequest,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = 
								$"isRefreshValid = {IsRefreshValid(req, user)} " +
								$"| userRole = {userRole} " +
								$"| isUserNull = {isUserNull}" +
								$"| isRefreshTokenValid = {isRefreshTokenValid}" +
								$"| isRefreshTokenExpired = {isRefreshTokenExpired}",
						}
					},
				};
			}

			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.FirstOrDefaultAsync(e => e.User == user);

			if (employee == null &&	user.Email != "master@example.com")
			{
				return new AuthResponse
				{
					Status = HttpStatusCode.NotFound,
					Errors = new List<IdentityError>
					{
						new IdentityError
						{
							Code = "Custom",
							Description = ServiceErrors.NoEmployeeError,
						}
					}
				};
			}

			var newAccessToken = GenerateAccessToken(user, employee, userRole);
			var newRefreshToken = GenerateRefreshToken();

			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(REFRESH_EXPIRY_MINUTES);

			await _userManager.UpdateAsync(user);

			return new AuthResponse
			{
				Status = HttpStatusCode.OK,
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken
			};
		}

		#endregion

		// ==== DEBUGGING ONLY ==========
		public async Task<AuthResponse> DEBUG_REGISTER(RegisterRequest req)
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
				//AccessToken = ...
			};
		}

		public async Task<ServiceResult> DEBUG_DELETE(string email)
		{
			var existingUser = await _userManager.FindByEmailAsync(email);

			if (existingUser == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeUserError
				};
			}

			await _userManager.DeleteAsync(existingUser);

			return new ServiceResult
			{
				Success = true,
			};
		}
	}
}

