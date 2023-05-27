using Capstone.Data;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Features.FeedbackModule.Models;
using Capstone.Features.PositionModule.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Capstone.ResultsAndResponses.SortParams;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Features.FeedbackModule
{
	public interface IFeedbackService
	{
		Task<PagedResult<FeedbackResponse>> GetFeedbacks(PagingParams pagingParams, SortParams sortParams);

		Task<ServiceResult> AddFeedback(AddFeedbackRequest req);
	}

	public class FeedbackService: IFeedbackService
	{
		private readonly CapstoneContext _context;
		public FeedbackService(CapstoneContext context)
		{
			_context = context;
		}
		#region==== Web ====

		public async Task<PagedResult<FeedbackResponse>> GetFeedbacks(PagingParams pagingParams, SortParams sortParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var queryableFilteredFeedbackResponses = _context.Feedbacks
				.Include(f => f.Employee)
				.Select(f => new FeedbackResponse
				{
					Title = f.Title,
					Description = f.Description,
					CreatedDate = f.CreatedDate,
					EmployeeFullName = f.Employee.FullName,
				});

			var sortedQueryableFilteredFeedbackResponses = 
				sortParams.SortDirection == SortDirection.Ascending ?
					(sortParams.SortByField) switch
					{
						"EmployeeFullName" => queryableFilteredFeedbackResponses
							.OrderBy(e => e.EmployeeFullName),
						"CreatedDate" => queryableFilteredFeedbackResponses
							.OrderBy(e => e.CreatedDate),
						_ => throw new ArgumentOutOfRangeException(nameof(sortParams.SortByField)),
					}
				: sortParams.SortDirection == SortDirection.Descending ?
					(sortParams.SortByField) switch
					{
						"EmployeeFullName" => queryableFilteredFeedbackResponses
							.OrderByDescending(e => e.EmployeeFullName),
						"CreatedDate" => queryableFilteredFeedbackResponses
							.OrderByDescending(e => e.CreatedDate),
						_ => throw new ArgumentOutOfRangeException(nameof(sortParams.SortByField)),
					}
				: queryableFilteredFeedbackResponses;

			var pagedFeedbackResponses = await sortedQueryableFilteredFeedbackResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await sortedQueryableFilteredFeedbackResponses.CountAsync();

			return new PagedResult<FeedbackResponse>(
				items: pagedFeedbackResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		#endregion

		#region==== Mobile ====

		public async Task<ServiceResult> AddFeedback(AddFeedbackRequest req)
		{
			var employee = await _context.People.OfType<Employee>()
				.SingleOrDefaultAsync(e => e.NationalId == req.EmployeeNationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError,
				};
			}

			var feedback = new Feedback
			{
				Title = req.Title,
				Description = req.Description,
				CreatedDate = DateTimeOffset.UtcNow,

				EmployeeId = employee.Id,
				Employee = employee,
			};

			await _context.Feedbacks.AddAsync(feedback);
			await _context.SaveChangesAsync();

			return new ServiceResult { Success = true };
		}

		#endregion
	}
}
