using Capstone.Features.Auth.Models;

namespace Capstone.Features.Auth
{
    public interface IAuthService
	{
		Task<AuthResponse> Register(RegisterRequest req);

		Task<AuthResponse> Login(LoginRequest req);
	}
}
