using Capstone.Data;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Features.LeaveModule.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Features.LeaveModule
{
	public interface ILeaveService
	{
		Task<PagedResult<LeaveResponse>> GetLeavesOfEmployee(PagingParams pagingParams, string NationalId);
		Task<ServiceResult> AddLeave(string NationalId, LeaveRequest req);

		Task<bool> CheckIfOnLeave(string NationalId, DateTimeOffset date);

		Task<bool> DEBUG_DELETE();
	}

	public class LeaveService: ILeaveService
	{
		private readonly CapstoneContext _context;

		public LeaveService(CapstoneContext context)
		{
			_context = context;
		}

		#region==== Web ====
		public async Task<PagedResult<LeaveResponse>> GetLeavesOfEmployee(PagingParams pagingParams, 
			string NationalId)
		{
			//var employee = await _context.People.OfType<Employee>()
			//	.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			//if (employee == null)
			//{
			//	return new ServiceResult
			//	{
			//		Success = false,
			//		ErrorMessage = ServiceErrors.NoEmployeeError,
			//	};
			//}

			var leaveResponses = await _context.Leaves
				.Include(l => l.Employee)
				.Where(l => l.Employee.NationalId == NationalId)
				.Select(l => new LeaveResponse
				{
					StartDate = l.StartDate,
					EndDate = l.EndDate,
					EmployeeNationalId = l.Employee.NationalId,
				})
				.ToListAsync();

			return new PagedResult<LeaveResponse>(leaveResponses, leaveResponses.Count, pagingParams);
		}

		public async Task<ServiceResult> AddLeave(string NationalId, LeaveRequest req)
		{
			var Start = req.StartDate.Date;
			var End = req.EndDate.Date;
			if (Start > End)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.StartAfterEndDateError,
				};
			}

			var employee = await _context.People.OfType<Employee>()
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError,
				};
			}

			var leave = new Leave
			{
				StartDate = Start,
				EndDate = End,
				Employee = employee,
				EmployeeId = employee.Id,
			};

			await _context.Leaves.AddAsync(leave);
			await _context.SaveChangesAsync();

			return new ServiceResult { Success = true };
		}
		#endregion

		#region==== Mobile ====
		public async Task<bool> CheckIfOnLeave(string NationalId, DateTimeOffset date)
		{
			var employee = await _context.People.OfType<Employee>()
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return false;
			}

			var isDateInAnyLeave = await _context.Leaves
				.Include(l => l.Employee)
				.Where(l => l.Employee.NationalId == NationalId)
				.AnyAsync(l =>
					(l.StartDate.Date <= date.Date) &&
					(l.EndDate.Date >= date.Date));

			return isDateInAnyLeave;
		}
		#endregion

		#region
		public async Task<bool> DEBUG_DELETE()
		{
			_context.Leaves.RemoveRange(await _context.Leaves.ToListAsync());
			await _context.SaveChangesAsync();

			return true;
		}
		#endregion
	}
}
