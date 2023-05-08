using Capstone.Data;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Features.FeedbackModule.Models;
using Capstone.Features.LeaveModule.Models;
using Capstone.Features.RequestModule.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Features.RequestModule
{
	public interface IRequestService
	{
		Task<PagedResult<RequestResponse>> GetRequests(PagingParams pagingParams);
		Task<ServiceResult> UpdateRequestStatusRequest(UpdateRequestStatusRequest req);

		Task<List<RequestResponse>> GetSelfRequests(string NationalId);
		Task<ServiceResult> AddRequest(AddRequestRequest req);
	}
	public class RequestService : IRequestService
	{
		private readonly CapstoneContext _context;

		public RequestService(CapstoneContext context)
		{
			_context = context;
		}

		#region==== Web ====
		public async Task<PagedResult<RequestResponse>> GetRequests(PagingParams pagingParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var queryableFilteredRequestResponses = _context.Requests
				.Include(f => f.Employee)
				.Select(f => new RequestResponse
				{
					Id = f.Id,
					Title = f.Title,
					Description = f.Description,
					CreatedDate = f.CreatedDate,
					Type = f.Type,
					RequestStatus = f.RequestStatus,
					NewSalary = f.NewSalary,
					StartLeaveDate = f.StartLeaveDate,
					EndLeaveDate = f.EndLeaveDate,
					EmployeeFullName = f.Employee.FullName,
				});

			var pagedRequestResponses = await queryableFilteredRequestResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredRequestResponses.CountAsync();

			return new PagedResult<RequestResponse>(
				items: pagedRequestResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<ServiceResult> UpdateRequestStatusRequest(UpdateRequestStatusRequest req)
		{
			var request = await _context.Requests
				.Include(r => r.Employee)
				.FirstOrDefaultAsync(r => r.Id == req.Id);

			if (request == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoRequestError,
				};
			}

			request.RequestStatus = req.RequestStatus;
			await _context.SaveChangesAsync();

			if (request.RequestStatus == RequestStatus.Rejected)
			{
				return new ServiceResult { Success = true };
			}

			var employee = request.Employee;

			switch (request.Type)
			{
				case "Raise":
					if (request.NewSalary == null)
					{
						return new ServiceResult
						{
							Success = false,
							ErrorMessage = ServiceErrors.NoNewSalaryError,
						};
					}

					employee.Salary = (int) request.NewSalary;
					break;

				case "Leave":
					if (request.StartLeaveDate == null || request.EndLeaveDate == null)
					{
						return new ServiceResult
						{
							Success = false,
							ErrorMessage = ServiceErrors.NoLeaveDateError,
						};
					}

					var leave = new Leave
					{
						StartDate = (DateTimeOffset) request.StartLeaveDate,
						EndDate = (DateTimeOffset) request.EndLeaveDate,
						EmployeeId = employee.Id,
						Employee = employee,
					};

					await _context.Leaves.AddAsync(leave);
					break;

				case "Other":
					break;
				default:
					break;
			}

			await _context.SaveChangesAsync();

			return new ServiceResult { Success = true };
		}
		#endregion

		#region==== Mobile ====
		public async Task<List<RequestResponse>> GetSelfRequests(string NationalId)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.Requests)
				.FirstOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return new List<RequestResponse>();
			}

			var requestResponses = employee.Requests
				.Select(r => new RequestResponse
				{
					Id = r.Id,
					CreatedDate = r.CreatedDate,
					Title = r.Title,
					Description = r.Description,
					EmployeeFullName = employee.FullName,
					Type = r.Type,
					RequestStatus = r.RequestStatus,

					NewSalary = r.NewSalary,
					StartLeaveDate = r.StartLeaveDate,
					EndLeaveDate = r.EndLeaveDate,
				})
				.OrderByDescending(r => r.Id)
				.ToList();

			return requestResponses;
		}

		public async Task<ServiceResult> AddRequest(AddRequestRequest req)
		{
			var employee = _context.People.OfType<Employee>()
				.FirstOrDefault(e => e.NationalId == req.EmployeeNationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
				};
			}

			var request = new Request
			{
				Title = req.Title,
				Description = req.Description,
				Type = req.Type,
				CreatedDate = DateTimeOffset.UtcNow,
				RequestStatus = RequestStatus.Pending,

				NewSalary = req.NewSalary,
				StartLeaveDate = req.StartLeaveDate,
				EndLeaveDate = req.EndLeaveDate,

				EmployeeId = employee.Id,
				Employee = employee
			};

			await _context.Requests.AddAsync(request);
			await _context.SaveChangesAsync();

			return new ServiceResult { Success = true };
		}
		#endregion
	}
}
