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
		// api/EmployeeModule
		//				?page=1&pageSize=10
		//				?SubName&Gender&Address&ExperienceYears&PositionModule&EmployedDate&Salary
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetEmployees(
			int? page, int? pageSize,
			string? SubName, string? Gender, string? Address, int? ExperienceYears,
			string? PositionName, DateTimeOffset? EmployedDateFrom, DateTimeOffset? EmployedDateTo, int? Salary)
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
			EmployeeFilterParams filterParams = new EmployeeFilterParams
			{
				SubName = SubName,
				Gender = Gender,
				Address = Address,
				ExperienceYears = ExperienceYears,
				PositionName = PositionName,
				EmployedDateFrom = EmployedDateFrom,
				EmployedDateTo = EmployedDateTo,
				Salary = Salary
			};

			var pagedEmployeeResponses = await _service
				.GetEmployees(pagingParams, filterParams);

			return Ok(pagedEmployeeResponses);
		}

		// GET: api/EmployeeModule/012012012
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

		// POST: api/EmployeeModule/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<ActionResult<EmployeeRequest>> PostEmployee(EmployeeRequest employeeReq)
		{
			await _service.AddEmployee(employeeReq);

            return CreatedAtAction(
				actionName: "GetEmployee", 
				routeValues: new { NationalId = employeeReq.NationalId },
				value: employeeReq
			);
        }

		// PUT: api/EmployeeModule/Update/012012012
		[HttpPut("Update/{NationalId}")]
		// NOTE, WIP: EmployeeModule can update itself, but NOT other Employees
		// check by current logged in user?
		[Authorize]
		public async Task<IActionResult> PutEmployee(
			[FromRoute] string NationalId, 
			[FromBody] EmployeeRequest employeeReq)
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

		// DELETE: api/EmployeeModule/Delete/012012012
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
