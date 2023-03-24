using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Capstone.Features.Auth.Models
{
    public class AuthResponse
    {
        public HttpStatusCode Status { get; set; }
        
		public IEnumerable<IdentityError>? Errors { get; set; }

		public string? AccessToken { get; set; }

		public string? RefreshToken { get; set; }
    }
}
