namespace Capstone.Features.Auth.Models
{
	public class RefreshRequest
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;

	}
}
