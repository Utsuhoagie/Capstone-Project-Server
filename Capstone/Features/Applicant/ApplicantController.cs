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

namespace Capstone.Features.ApplicantModule
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantController : ControllerBase
    {
        private readonly IApplicantService _service;

        public ApplicantController(IApplicantService service)
        {
			_service = service;
        }

		// GET:
		// api/ApplicantTracking
		//						?page=1&pageSize=10
		//						?SubName&Gender&Address&ExperienceYears&AppliedPosition&AppliedDate&AskingSalary
		[HttpGet]
		public async Task<IActionResult> GetApplicants(
			int? page, int? pageSize,
			string? SubName, string? Gender, string? Address, int? ExperienceYears,
			string? AppliedPosition, DateTimeOffset? AppliedDateFrom, DateTimeOffset? AppliedDateTo, int? AskingSalary)
		{
			if (page == null || pageSize == null)
			{
				return Ok(await _service.GetAllApplicantsAsync());
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
				AppliedPosition = AppliedPosition,
				AppliedDateFrom = AppliedDateFrom,
				AppliedDateTo = AppliedDateTo,
				AskingSalary = AskingSalary
			};

			var applicants = await _service
				.GetApplicantsAsync(pagingParams, filterParams);

			return Ok(applicants);
		}

		/*[HttpGet]
		public async Task<IActionResult> GetApplicants()
		{
			return Ok(await _service.GetAllApplicantsAsync());
		}*/

		// GET: api/ApplicantTracking/012012012
		[HttpGet("{NationalId}")]
		public async Task<ActionResult<ApplicantDto>> GetApplicant(string NationalId)
		{
			var applicant = await _service.GetApplicantAsync(NationalId);

			if (applicant == null)
			{
				return NotFound();
			}

			return Ok(applicant);
		}

		// POST: api/ApplicantTracking/Create
		[HttpPost("Create")]
		public async Task<ActionResult<ApplicantDto>> PostApplicant(ApplicantDto applicantDto)
		{
			await _service.AddApplicantAsync(applicantDto);

            return CreatedAtAction(
				actionName: "GetApplicant", 
				routeValues: new { NationalId = applicantDto.NationalId },
				value: applicantDto
			);
        }

		// PUT: api/ApplicantTracking/Update?NationalId=<string>
		[HttpPut("Update")]
		public async Task<IActionResult> PutApplicant(
			[FromQuery] string NationalId, 
			[FromBody] ApplicantDto applicantDto)
		{
			if (NationalId != applicantDto.NationalId)
			{
				return BadRequest();
			}

			var result = await _service.UpdateApplicantAsync(NationalId, applicantDto);

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

		// DELETE: api/ApplicantTracking/Delete?NationalId={string}
		[HttpDelete("Delete")]
        public async Task<IActionResult> DeleteApplicant([FromQuery] string? NationalId)
        {
			if (NationalId == null)
			{
				await _service.DeleteApplicantsAsync();

				return NoContent();
			}

			var result = await _service.DeleteApplicantAsync(NationalId);
            
			if (result == false)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
