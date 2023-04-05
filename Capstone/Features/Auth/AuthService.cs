using Capstone.Data;
using Capstone.Features.Auth.Models;
using Capstone.Models;
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
    public class AuthService: IAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly CapstoneContext _context;
		private readonly UserManager<EmployeeUser> _userManager;
		private readonly IValidator<RegisterRequest> _registerValidator;

		private readonly int ACCESS_EXPIRY_MINUTES = 15;
		private readonly int REFRESH_EXPIRY_MINUTES = 60;

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
		
		// Auth internal methods
		public string GenerateAccessToken(EmployeeUser user, string userRole)
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
			};

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
				ValidAudience = _configuration.GetSection("JWT:ValidAudience").Value,
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

		// Auth services
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
				Email = req.Email
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
					Status = HttpStatusCode.Unauthorized,
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

			var isUserValid = await _userManager.CheckPasswordAsync(existingUser, req.Password);

			if (!isUserValid)
			{
				return new AuthResponse { Status = HttpStatusCode.Unauthorized };
			}

			var userRoles = await _userManager.GetRolesAsync(existingUser);
			var userRole = userRoles.SingleOrDefault()!;

			var accessToken = GenerateAccessToken(existingUser, userRole);
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
			var userEmail = (principal.Identity!).Name;

			var user = await _userManager.FindByEmailAsync(userEmail);
			var userRole = (await _userManager.GetRolesAsync(user)).SingleOrDefault();

			if (!IsRefreshValid(req, user) || userRole == null)
			{
				return new AuthResponse { Status = HttpStatusCode.BadRequest };
			}

			var newAccessToken = GenerateAccessToken(user, userRole);
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

