using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capstone.Data;
using Capstone.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Capstone.Features.Auth;
using Capstone.Responses.Pagination;

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
		//				?SubName&Gender&Address&ExperienceYears&Position&EmployedDate&Salary
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

			var employeeDtos = await _service
				.GetEmployees(pagingParams, filterParams);

			return Ok(employeeDtos);
		}

		// GET: api/Employee/012012012
		[HttpGet("{NationalId}")]
		[Authorize]
		public async Task<ActionResult<EmployeeDto>> GetEmployee([FromRoute] string NationalId)
		{
			var employeeDto = await _service.GetEmployee(NationalId);

			if (employeeDto == null)
			{
				return NotFound();
			}

			return Ok(employeeDto);
		}

		// POST: api/Employee/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<ActionResult<EmployeeDto>> PostEmployee(EmployeeDto employeeDto)
		{
			await _service.AddEmployee(employeeDto);

            return CreatedAtAction(
				actionName: "GetEmployee", 
				routeValues: new { NationalId = employeeDto.NationalId },
				value: employeeDto
			);
        }

		// PUT: api/Employee/Update/012012012
		[HttpPut("Update/{NationalId}")]
		// NOTE, WIP: Employee can update itself, but NOT other Employees
		// check by current logged in user?
		[Authorize]
		public async Task<IActionResult> PutEmployee(
			[FromRoute] string NationalId, 
			[FromBody] EmployeeDto employeeDto)
		{
			if (NationalId != employeeDto.NationalId)
			{
				return BadRequest();
			}

			var result = await _service.UpdateEmployee(NationalId, employeeDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

			return NoContent();
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
