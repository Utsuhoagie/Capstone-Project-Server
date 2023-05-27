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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Capstone.Responses.Pagination;
using Capstone.Features.PositionModule.Models;

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
		//				?SubName&Gender&Address&ExperienceYears&PositionModule&AppliedDate&Salary
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
		public async Task<ActionResult<PositionResponse>> GetPosition([FromRoute] string Name)
		{
			var res = await _service.GetPosition(Name);

			if (res == null)
			{
				return NotFound();
			}

			return Ok(res);
		}

		// POST: api/Positions/Create
		[HttpPost("Create")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<ActionResult<PositionResponse>> PostPosition(PositionRequest req)
		{
			var result = await _service.AddPosition(req);

			if (result.Success == false)
			{
				return BadRequest(result.ErrorMessage);
			}

            return CreatedAtAction(
				actionName: "GetPosition", 
				routeValues: new { Name = req.Name },
				value: req
			);
        }

		// PUT: api/Positions/Update/HR
		[HttpPut("Update/{Name}")]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> PutPosition(
			[FromRoute] string Name, 
			[FromBody] PositionRequest req)
		{
			//if (Name != positionRes.Name)
			//{
			//	return BadRequest();
			//}

			var result = await _service.UpdatePosition(Name, req);

			if (result.Success == false)
			{
				return BadRequest(result.ErrorMessage);
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
