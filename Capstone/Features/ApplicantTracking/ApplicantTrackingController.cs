using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capstone.Data;
using Capstone.Models;
using Capstone.Features.ApplicantTracking;

namespace Capstone.Features.ApplicantTracking
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantTrackingController : ControllerBase
    {
        private readonly IApplicantTrackingService _service;

        public ApplicantTrackingController(IApplicantTrackingService service)
        {
            _service = service;
        }

        // GET: api/ApplicantTracking
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicantDto>>> GetApplicant()
        {
            var results = await _service.GetApplicantsAsync();

            return Ok(results);
        }

        // GET: api/ApplicantTracking/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicantDto>> GetApplicant(int id)
        {
            var applicant = await _service.GetApplicantAsync(id);

            if (applicant == null)
            {
                return NotFound();
            }

            return applicant;
        }

        // POST: api/ApplicantTracking
        [HttpPost]
		//public async Task<ActionResult<ApplicantDto>> PostApplicant(ApplicantDto applicantDto)
		public async Task<ActionResult<object>> PostApplicant(ApplicantDto applicantDto)
		{
			await _service.AddApplicantAsync(applicantDto);

            return CreatedAtAction(
				actionName: "GetApplicant", 
				//routeValues: new { id = applicantDto.Id },
				value: applicantDto
			);
        }

		// PUT: api/ApplicantTracking/5
		/*        [HttpPut("{id}")]
				public async Task<IActionResult> PutApplicant(int id, Applicant applicant)
				{
					if (id != applicant.Id)
					{
						return BadRequest();
					}

					_context.Entry(applicant).State = EntityState.Modified;

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
					}

					return NoContent();
				}*/

		// DELETE: api/ApplicantTracking
		[HttpDelete]
		public async Task<IActionResult> DeleteApplicants()
		{
			await _service.DeleteApplicantsAsync();

			return NoContent();
		}

        // DELETE: api/ApplicantTracking/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicant(int id)
        {
			var result = await _service.DeleteApplicantAsync(id);
            
			if (result == false)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
