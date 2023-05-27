using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.Auth;
using Capstone.Features.FileModule;
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

		#region==== Web ====
		[HttpGet("DailyHash")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetDailyHash()
		{
			var dailyHash = _service.GetDailyHash();
			return Ok(dailyHash);
		}

		// GET: api/Attendances/Monthly?date=2023-04-07T02:04:29.000Z
		[HttpGet("DailyAttendanceStatusesOfMonth")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetDailyAttendanceStatusesOfMonth(DateTimeOffset date)
		{
			var vnDate = date.ToOffset(new TimeSpan(7, 0, 0));
			var monthAttendanceStatus = await _service.GetDailyAttendanceStatusesOfMonth(vnDate);

			return Ok(monthAttendanceStatus);
		}

		// GET: api/Attendances/EmployeesNotOnLeave?page=1&pageSize=30&date=2023-04-07T02:04:29.000Z
		[HttpGet("EmployeesNotOnLeave")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetEmployeesNotOnLeave(
			int page, int pageSize,
			DateTimeOffset date)
		{
			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = page, PageSize = pageSize };
			DateTimeOffset vnDate = date.ToOffset(new TimeSpan(7, 0, 0));

			var pagedEmployeeResponses = await _service
				.GetEmployeesNotOnLeave(pagingParams, vnDate);

			return Ok(pagedEmployeeResponses);
		}

		[HttpGet("AttendanceOfEmployee")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetAttendanceOfEmployee(string NationalId, DateTimeOffset date)
		{
			var vnDate = date.ToOffset(new TimeSpan(7, 0, 0));
			var result = await _service.GetAttendanceOfEmployee(NationalId, vnDate);

			if (result == null)
			{
				return Ok(null);
			}

			return Ok(result);
		}

		[HttpPut("BatchUpdateStatuses")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> BatchUpdateStatuses(
			[FromQuery] string type,
			[FromQuery] string dayOrMonth,
			[FromQuery] DateTimeOffset date)
		{
			if ((type != "Accept" && type != "Reject") || (dayOrMonth != "day" && dayOrMonth != "month"))
			{
				return BadRequest("Hành động không hợp lệ.");
			}

			var vnDate = date.ToOffset(new TimeSpan(7, 0, 0));

			var result = await _service.BatchUpdateStatuses(type, dayOrMonth, vnDate);

			if (!result.Success)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result);
		}

		[HttpPut("UpdateStatus")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> UpdateStatus(UpdateStatusRequest req)
		{
			var result = await _service.UpdateStatus(req);

			if (!result.Success)
			{
				return BadRequest(result.ErrorMessage);
			}

			return Ok(result);
		}
		#endregion

		#region==== Mobile ====
		[HttpGet("CheckAttendanceToday")]
		[Authorize]
		public async Task<IActionResult> CheckAttendanceToday(
			[FromQuery] string NationalId,
			[FromQuery] DateTimeOffset date)
		{
			var vnDate = date.ToOffset(new TimeSpan(7, 0, 0));
			var result = await _service.CheckAttendanceToday(NationalId, vnDate);

			if (result == null)
			{
				return BadRequest();
			}

			return Ok(result);
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
		#endregion

		[HttpDelete]
		public async Task<IActionResult> DEBUG_DELETE()
		{
			return Ok(await _service.DEBUG_DELETE());
		}
	}
}
