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
using Capstone.Pagination;
using Microsoft.AspNetCore.Authorization;
using Capstone.Features.Auth;

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
		//				?SubName&Gender&Address&ExperienceYears&Position&AppliedDate&Salary
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetEmployees(
			int? page, int? pageSize,
			string? SubName, string? Gender, string? Address, int? ExperienceYears,
			string? AppliedPosition, DateTimeOffset? AppliedDateFrom, DateTimeOffset? AppliedDateTo, int? AskingSalary)
		{
			if (page == null || pageSize == null)
			{
				return Ok(await _service.GetAllEmployeesAsync());
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
				Position = AppliedPosition,
				EmployedDateFrom = AppliedDateFrom,
				EmployedDateTo = AppliedDateTo,
				Salary = AskingSalary
			};

			var employeeDtos = await _service
				.GetEmployeesAsync(pagingParams, filterParams);

			return Ok(employeeDtos);
		}

		/*[HttpGet]
		public async Task<IActionResult> GetEmployees()
		{
			return Ok(await _service.GetAllEmployeesAsync());
		}*/

		// GET: api/Employee/012012012
		[HttpGet("{NationalId}")]
		[Authorize]
		public async Task<ActionResult<EmployeeDto>> GetEmployee(string NationalId)
		{
			var employeeDto = await _service.GetEmployeeAsync(NationalId);

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
			await _service.AddEmployeeAsync(employeeDto);

            return CreatedAtAction(
				actionName: "GetEmployee", 
				routeValues: new { NationalId = employeeDto.NationalId },
				value: employeeDto
			);
        }

		// PUT: api/Employee/Update?NationalId=<string>
		[HttpPut("Update")]
		// NOTE, WIP: Employee can update itself, but NOT other Employees
		// check by current logged in user?
		[Authorize]
		public async Task<IActionResult> PutEmployee(
			[FromQuery] string NationalId, 
			[FromBody] EmployeeDto employeeDto)
		{
			if (NationalId != employeeDto.NationalId)
			{
				return BadRequest();
			}

			var result = await _service.UpdateEmployeeAsync(NationalId, employeeDto);

			if (result == false)
			{
				return BadRequest();
			}

			return NoContent();

			// DEFAULT GENERATED
/*			_context.Entry(applicant).State = EntityState.Modified;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ApplicantExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}*/
		}

		// DELETE: api/Employee/Delete?NationalId={string}
		[HttpDelete("Delete")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> DeleteEmployee([FromQuery] string? NationalId)
        {
			if (NationalId == null)
			{
				await _service.DeleteAllEmployeesAsync();

				return NoContent();
			}

			var result = await _service.DeleteEmployeeAsync(NationalId);
            
			if (result == false)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
