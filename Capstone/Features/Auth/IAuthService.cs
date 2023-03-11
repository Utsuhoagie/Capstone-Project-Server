using Capstone.Features.Auth.Models;

namespace Capstone.Features.Auth
{
    public interface IAuthService
	{
		Task<AuthResponse> Login(LoginRequest req);

		/*Task<AuthResponse> Register(RegisterRequest req);*/

		Task<bool> DEBUG_DELETE(string email);
	}
}
