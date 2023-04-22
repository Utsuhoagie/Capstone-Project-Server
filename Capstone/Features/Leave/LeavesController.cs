using Capstone.Features.Auth;
using Capstone.Features.LeaveModule.Models;
using Capstone.Responses.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Features.LeaveModule
{
	[Route("api/[controller]")]
	[ApiController]
	public class LeavesController : ControllerBase
	{
		private readonly ILeaveService _service;

		public LeavesController(ILeaveService service)
		{
			_service = service;
		}

		#region==== Web ====
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetLeavesOfEmployee(int page, int pageSize, string NationalId)
		{
			PagingParams pagingParams = new PagingParams
			{
				Page = page,
				PageSize = pageSize,
			};

			return Ok(await _service.GetLeavesOfEmployee(pagingParams, NationalId));
		}

		[HttpPost("Create/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> AddLeave([FromRoute] string NationalId,
			[FromBody] LeaveRequest req)
		{
			var result = await _service.AddLeave(NationalId, req);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}
		#endregion

		#region
		// GET: /Leaves/Check?NationalId=000000001&date=2023-04-19T10:37:16.914Z
		[HttpGet("Check")]
		[Authorize]
		public async Task<IActionResult> CheckIsOnLeave([FromQuery] string NationalId, 
			[FromQuery] DateTimeOffset date)
		{
			var result = await _service.CheckIfOnLeave(NationalId, date);

			return Ok(new { IsOnLeave = result });
		}
		#endregion
	}
}
