using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.Auth;
using Capstone.Features.File;
using Capstone.Models;
using Capstone.Responses.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone.Features.AttendanceModule
{
    [Route("api/[controller]")]
	[ApiController]
	public class AttendancesController : ControllerBase
	{
		private readonly IAttendanceService _service;
		private readonly IConfiguration _configuration;
		public AttendancesController(IAttendanceService attendanceService, IConfiguration configuration) 
		{
			_service = attendanceService;
			_configuration = configuration;
		}

		[HttpGet("DailyHash")]
		[Authorize]
		public async Task<IActionResult> GetDailyHash()
		{
			var dailyHash = _service.GetDailyHash();
			return Ok(dailyHash);
		}

		// GET: api/Attendances/Monthly?date=2023-04-07T02:04:29.000Z
		[HttpGet("DailyAttendanceStatusesOfMonth")]
		public async Task<IActionResult> GetDailyAttendanceStatusesOfMonth(DateTimeOffset date)
		{
			var vnDate = date.AddHours(7);
			DateOnly dateOnly = new DateOnly(vnDate.Year, vnDate.Month, vnDate.Day);
			var monthAttendanceStatus = await _service.GetDailyAttendanceStatusesOfMonth(dateOnly);

			return Ok(monthAttendanceStatus);
		}

		// GET: api/Attendances/Daily?page=1&pageSize=30&date=2023-04-07T02:04:29.000Z
		[HttpGet("Daily")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetDailyAttendances(
			int page, int pageSize,
			DateTimeOffset date)
		{
			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = page, PageSize = pageSize };
			var vnDate = date.AddHours(7);
			DateOnly dateOnly = new DateOnly(vnDate.Year, vnDate.Month, vnDate.Day);

			var pagedAttendanceDtos = await _service
				.GetDailyAttendances(pagingParams, dateOnly);
			
			return Ok(pagedAttendanceDtos);
		}

		[HttpPost("Start")]
		[Authorize]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> StartAttendance([FromForm] StartAttendanceRequest req)
		{
			var startImage = req.StartImage;

			if (startImage.Length <= 0)
			{
				return BadRequest("Hình ảnh không hợp lệ.");
			}

			var result = await _service.StartAttendance(req);

			if (result.Success == false)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(/*new
			{
				Message = $"File is uploaded at {req.StartTimestamp}. Success? {result.Success}",
				Image = startImage
			}*/);
		}

		[HttpPost("End")]
		[Authorize]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> EndAttendance([FromForm] EndAttendanceRequest req)
		{
			var endImage = req.EndImage;

			if (endImage.Length <= 0)
			{
				return BadRequest("Hình ảnh không hợp lệ.");
			}

			var result = await _service.EndAttendance(req);

			if (result.Success == false)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(/*new
			{
				Message = $"File is uploaded at {req.EndTimestamp}. Success? {result.Success}",
				Image = endImage
			}*/);
		}

		[HttpPut("UpdateStatus")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> UpdateStatus(UpdateStatusRequest req)
		{
			var result = await _service.UpdateStatus(req);

			if (result.Success)
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
	}
}
