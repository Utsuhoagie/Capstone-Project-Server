using Capstone.Features.RequestModule.Models;

namespace Capstone.Features.RequestModule.Models
{
	public class RequestParams
	{
		public string? Type { get; set; }
		public RequestStatus? RequestStatus { get; set; }
	}
}
