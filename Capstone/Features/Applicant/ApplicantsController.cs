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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Capstone.Responses.Pagination;

namespace Capstone.Features.ApplicantModule
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantsController : ControllerBase
    {
        private readonly IApplicantService _service;

        public ApplicantsController(IApplicantService service)
        {
			_service = service;
        }

		// GET:
		// api/Applicants
		//				?page=1&pageSize=10
		//				?SubName&Gender&Address&ExperienceYears&Position&AppliedDate&Salary
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetApplicants(
			int? page, int? pageSize,
			string? SubName, string? Gender, string? Address, int? ExperienceYears,
			string? AppliedPosition, DateTimeOffset? AppliedDateFrom, DateTimeOffset? AppliedDateTo, int? AskingSalary)
		{
			if (page == null || pageSize == null)
			{
				return Ok(await _service.GetAllApplicants());
			}

			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = (int)page, PageSize = (int)pageSize };
			ApplicantFilterParams filterParams = new ApplicantFilterParams
			{
				SubName = SubName,
				Gender = Gender,
				Address = Address,
				ExperienceYears = ExperienceYears,
				AppliedPositionName = AppliedPosition,
				AppliedDateFrom = AppliedDateFrom,
				AppliedDateTo = AppliedDateTo,
				AskingSalary = AskingSalary
			};

			var applicantDtos = await _service
				.GetApplicants(pagingParams, filterParams);

			return Ok(applicantDtos);
		}

		// GET: api/Applicants/012012012
		[HttpGet("{NationalId}")]
		[Authorize]
		public async Task<ActionResult<ApplicantDto>> GetApplicant([FromRoute] string NationalId)
		{
			var applicantDto = await _service.GetApplicant(NationalId);

			if (applicantDto == null)
			{
				return NotFound();
			}

			return Ok(applicantDto);
		}

		// POST: api/Applicants/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<ActionResult<ApplicantDto>> PostApplicant(ApplicantDto applicantDto)
		{
			var result = await _service.AddApplicant(applicantDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

            return CreatedAtAction(
				actionName: "GetApplicant", 
				routeValues: new { NationalId = applicantDto.NationalId },
				value: applicantDto
			);
        }

		// PUT: api/Applicants/Update/012012012
		[HttpPut("Update/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> PutApplicant(
			[FromRoute] string NationalId, 
			[FromBody] ApplicantDto applicantDto)
		{
			if (NationalId != applicantDto.NationalId)
			{
				return BadRequest();
			}

			var result = await _service.UpdateApplicant(NationalId, applicantDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

			return NoContent();
		}

		// DELETE: api/Applicants/Delete/012012012
		[HttpDelete("Delete/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> DeleteApplicant([FromRoute] string? NationalId)
        {
			if (NationalId == null)
			{
				await _service.DeleteAllApplicants();

				return NoContent();
			}

			var result = await _service.DeleteApplicant(NationalId);
            
			if (result.Success == false)
            {
                return NotFound(result);
            }

            return NoContent();
        }

		// POST: api/Applicants/Employ/012012012
		[HttpPost("Employ/{NationalId}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> EmployApplicant(
			[FromRoute] string NationalId,
			[FromBody] EmployeeDto employeeDto)
		{
			var result = await _service.EmployApplicant(NationalId, employeeDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

			return Ok();
		}
    }
}
