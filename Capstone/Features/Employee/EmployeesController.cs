using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capstone.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Capstone.Features.Auth;
using Capstone.Responses.Pagination;
using Capstone.Features.EmployeeModule.Models;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Net;

namespace Capstone.Features.EmployeeModule
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
			_service = service;
        }

		// GET:
		// api/Employee
		//				?page=1&pageSize=10
		//				?SubName&Gender&Address&ExperienceYears&PositionName&EmployedDate&Salary
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetEmployees(
			[FromQuery] int? page, [FromQuery] int? pageSize,
			[FromQuery] EmployeeParams employeeParams)
		{
			if (page == null || pageSize == null)
			{
				return Ok(await _service.GetAllEmployees());
			}

			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = (int)page, PageSize = (int)pageSize };
			employeeParams.EmployedDateFrom = employeeParams.EmployedDateFrom?.ToOffset(new TimeSpan(7,0,0));
			employeeParams.EmployedDateTo = employeeParams.EmployedDateTo?.ToOffset(new TimeSpan(7,0,0));

			var pagedEmployeeResponses = await _service
				.GetEmployees(pagingParams, employeeParams);

			return Ok(pagedEmployeeResponses);
		}

		// GET: api/Employee/012012012
		[HttpGet("{NationalId}")]
		[Authorize]
		public async Task<ActionResult<EmployeeRequest>> GetEmployee([FromRoute] string NationalId)
		{
			var res = await _service.GetEmployee(NationalId);

			if (res == null)
			{
				return NotFound();
			}

			return Ok(res);
		}

		// POST: api/Employee/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		[Consumes("multipart/form-data")]
		public async Task<ActionResult<EmployeeRequest>> PostEmployee([FromForm] EmployeeRequest employeeReq)
		{
			await _service.AddEmployee(employeeReq);

            return CreatedAtAction(
				actionName: "GetEmployee", 
				routeValues: new { NationalId = employeeReq.NationalId },
				value: employeeReq
			);
        }

		// PUT: api/Employee/Update/012012012
		[HttpPut("Update/{NationalId}")]
		// NOTE, WIP: EmployeeModule can update itself, but NOT other Employees
		// check by current logged in user?
		[Authorize(Roles = AuthRoles.Admin)]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> PutEmployee(
			[FromRoute] string NationalId, 
			[FromForm] EmployeeRequest employeeReq)
		{
			if (NationalId != employeeReq.NationalId)
			{
				return BadRequest();
			}

			var result = await _service.UpdateEmployee(NationalId, employeeReq);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

			return NoContent();
		}

		[HttpPut("UpdateSelf/{NationalId}")]
		[Authorize(Roles = AuthRoles.Employee)]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> UpdateSelf(
			[FromRoute] string NationalId,
			[FromForm] UpdateSelfRequest req)
		{
			var result = await _service.UpdateSelf(NationalId, req);

			if (!result.Success)
			{
				return Forbid();
			}

			return Ok();

			/* Tried doing JWT claims-based verify for self to update only self
			var authorization = Request.Headers.Authorization;

			if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
			{
				// we have a valid AuthenticationHeaderValue that has the following details:
				// scheme will be "Bearer"
				// parmameter will be the token itself.
				var scheme = headerValue.Scheme;
				var parameter = headerValue.Parameter;

				return Ok(parameter);
			}

			return Ok();*/
		}

		// DELETE: api/Employee/Delete/012012012
		[HttpDelete("Delete/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> DeleteEmployee([FromRoute] string? NationalId)
        {
			if (NationalId == null)
			{
				await _service.DeleteAllEmployees();

				return NoContent();
			}

			var result = await _service.DeleteEmployee(NationalId);
            
			if (result.Success == false)
            {
                return NotFound(result);
            }

            return NoContent();
        }
    }
}
