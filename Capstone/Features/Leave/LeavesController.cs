﻿using Capstone.Features.Auth;
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

		#region==== Web & Mobile ====
		[HttpGet("{NationalId}")]
		[Authorize]
		public async Task<IActionResult> GetLeavesOfEmployee([FromRoute] string NationalId)
		{
			return Ok(await _service.GetLeavesOfEmployee(NationalId));
		}
		#endregion

		#region==== Mobile ====
		// GET: /Leaves/Check?NationalId=000000001&date=2023-04-19T10:37:16.914Z
		[HttpGet("Check")]
		[Authorize]
		public async Task<IActionResult> CheckIsOnLeave([FromQuery] string NationalId, 
			[FromQuery] DateTimeOffset date)
		{
			var vnDate = date.ToOffset(new TimeSpan(7, 0, 0));
			var result = await _service.CheckIfOnLeave(NationalId, vnDate);

			return Ok(new { IsOnLeave = result });
		}
		#endregion

		#region==== DEBUG ====
		[HttpPost("Create/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> AddLeave([FromRoute] string NationalId,
			[FromBody] LeaveRequest req)
		{
			var result = await _service.AddLeave(NationalId, req);

			if (!result.Success)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result);
		}

		[HttpDelete]
		public async Task<IActionResult> DEBUG_DELETE()
		{
			return Ok(await _service.DEBUG_DELETE());
		}
		#endregion
	}
}
