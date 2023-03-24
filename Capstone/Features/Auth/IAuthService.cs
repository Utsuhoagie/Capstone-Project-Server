using Capstone.Features.Auth.Models;
using Capstone.Models;
using Capstone.Responses.ServiceResponse;

namespace Capstone.Features.Auth
{
    public interface IAuthService
	{
		string GenerateAccessToken(EmployeeUser user, string userRole);

		string GenerateRefreshToken();

		Task<AuthResponse> RegisterEmployee(RegisterEmployeeRequest req);

		Task<AuthResponse> Login(LoginRequest req);

		Task<AuthResponse> Refresh(RefreshRequest req);

		Task<AuthResponse> DEBUG_REGISTER(RegisterRequest req);

		Task<ServiceResult> DEBUG_DELETE(string email);
	}
}
