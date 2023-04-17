using Capstone.Features.AttendanceModule.Models;
using Capstone.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using FluentValidation.Results;

namespace Capstone.Features.AttendanceModule
{
    public interface IAttendanceService
    {
		string GetDailyHash();
		Task<PagedResult<AttendanceDto>> GetDailyAttendances(PagingParams pagingParams, DateOnly dateOnly);
		Task<object> GetDailyAttendanceStatusesOfMonth(DateOnly dateOnly);
		Task<ServiceResult> StartAttendance(StartAttendanceRequest req);
		Task<ServiceResult> EndAttendance(EndAttendanceRequest req);
		Task<ServiceResult> UpdateStatus(UpdateStatusRequest req);

		//Task<PagedResult<ApplicantDto>> GetApplicants(PagingParams pagingParams, ApplicantFilterParams filterParams);

		//Task<ApplicantDto?> GetApplicant(string NationalId);

		//Task<ServiceResult> DeleteApplicant(string NationalId);

		//Task<ServiceResult> EmployApplicant(string NationalId, EmployeeDto employeeDto);

		Task<ServiceResult> DEBUG_DELETE();
	}
}
