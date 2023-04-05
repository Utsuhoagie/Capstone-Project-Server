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

namespace Capstone.Features.PositionModule
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _service;

        public PositionsController(IPositionService service)
        {
			_service = service;
        }

		// GET:
		// api/Positions
		//				?page=1&pageSize=10
		//				?SubName&Gender&Address&ExperienceYears&Position&AppliedDate&Salary
		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetPositions(int? page, int? pageSize)
		{
			if (page == null || pageSize == null)
			{
				return Ok(await _service.GetAllPositions());
			}

			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = (int)page, PageSize = (int)pageSize };

			var positionDtos = await _service
				.GetPositions(pagingParams);

			return Ok(positionDtos);
		}

		// GET: api/Positions/HR
		[HttpGet("{Name}")]
		[Authorize]
		public async Task<ActionResult<PositionDto>> GetPosition([FromRoute] string Name)
		{
			var positionDto = await _service.GetPosition(Name);

			if (positionDto == null)
			{
				return NotFound();
			}

			return Ok(positionDto);
		}

		// POST: api/Positions/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<ActionResult<PositionDto>> PostPosition(PositionDto positionDto)
		{
			var result = await _service.AddPosition(positionDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

            return CreatedAtAction(
				actionName: "GetPosition", 
				routeValues: new { Name = positionDto.Name },
				value: positionDto
			);
        }

		// PUT: api/Positions/Update/HR
		[HttpPut("Update/{Name}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> PutPosition(
			[FromRoute] string Name, 
			[FromBody] PositionDto positionDto)
		{
			//if (Name != positionDto.Name)
			//{
			//	return BadRequest();
			//}

			var result = await _service.UpdatePosition(Name, positionDto);

			if (result.Success == false)
			{
				return BadRequest(result);
			}

			return NoContent();
		}

		// DELETE: api/Positions/Delete/HR
		[HttpDelete("Delete/{Name}")]
		[Authorize(Roles = AuthRoles.Admin)]
        public async Task<IActionResult> DeletePosition([FromRoute] string? Name)
        {
			if (Name == null)
			{
				await _service.DeleteAllPositions();

				return NoContent();
			}

			var result = await _service.DeletePosition(Name);
            
			if (result.Success == false)
            {
                return NotFound(result);
            }

            return NoContent();
        }
    }
}
