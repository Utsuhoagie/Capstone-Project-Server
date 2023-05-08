using Capstone.Features.Auth;
using Capstone.Features.RequestModule.Models;
using Capstone.Responses.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone.Features.RequestModule
{
	[Route("api/[controller]")]
	[ApiController]
	public class RequestsController : ControllerBase
	{
		private readonly IRequestService _service;

		public RequestsController(IRequestService service)
		{
			_service = service;
		}

		#region==== Web ====
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetRequests(int page, int pageSize)
		{
			PagingParams pagingParams = new PagingParams { Page = page, PageSize = pageSize };

			var requestResponses = await _service.GetRequests(pagingParams);

			return Ok(requestResponses);
		}

		[HttpPut("UpdateRequestStatus")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> UpdateRequestStatusRequest(UpdateRequestStatusRequest req)
		{
			var result = await _service.UpdateRequestStatusRequest(req);

			if (!result.Success)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result);
		}
		#endregion

		#region==== Mobile ====
		[HttpGet("Self/{NationalId}")]
		[Authorize]
		public async Task<IActionResult> GetSelfRequests([FromRoute] string NationalId)
		{
			var requestResponses = await _service.GetSelfRequests(NationalId);

			return Ok(requestResponses);
		}

		// POST api/Requests/Create/000000001
		[HttpPost("Create")]
		[Authorize]
		public async Task<IActionResult> AddRequest(
			[FromBody] AddRequestRequest req)
		{
			var result = await _service.AddRequest(req);

			if (!result.Success)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result);
		}
		#endregion
	}
}
