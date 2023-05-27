using Capstone.Features.Auth;
using Capstone.Features.EmployeeModule;
using Capstone.Features.FeedbackModule.Models;
using Capstone.Responses.Pagination;
using Capstone.ResultsAndResponses.SortParams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Features.FeedbackModule
{
	[Route("api/[controller]")]
	[ApiController]
	public class FeedbacksController : ControllerBase
	{
		private readonly IFeedbackService _service;

		public FeedbacksController(IFeedbackService feedbackService)
		{
			_service = feedbackService;
		}

		#region==== Web ====

		[HttpGet]
		[Authorize(Roles = AuthRoles.Admin)]
		public async Task<IActionResult> GetFeedbacks(
			[FromQuery] int page, [FromQuery] int pageSize,
			[FromQuery] SortParams sortParams
		)
		{
			if (page < 1 || pageSize < 1)
			{
				return BadRequest();
			}

			PagingParams pagingParams = new PagingParams { Page = (int)page, PageSize = (int)pageSize };

			var pagedFeedbackResponses = await _service
				.GetFeedbacks(pagingParams, sortParams);

			return Ok(pagedFeedbackResponses);
		}

		#endregion

		#region==== Mobile ====

		[HttpPost("Create")]
		[Authorize]
		public async Task<IActionResult> AddFeedback(AddFeedbackRequest req)
		{
			var result = await _service.AddFeedback(req);

			if (!result.Success)
			{
				return BadRequest();
			}

			return Ok(req);
		}

		#endregion
	}
}
