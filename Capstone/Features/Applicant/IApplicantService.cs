using Capstone.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using FluentValidation.Results;

namespace Capstone.Features.ApplicantModule
{
    public interface IApplicantService
    {
		Task<PagedResult<ApplicantDto>> GetAllApplicants();

        Task<PagedResult<ApplicantDto>> GetApplicants(PagingParams pagingParams, ApplicantFilterParams filterParams);

		Task<ApplicantDto?> GetApplicant(string NationalId);

		Task<ServiceResult> AddApplicant(ApplicantDto applicantDto);

		Task<ServiceResult> UpdateApplicant(string NationalId, ApplicantDto applicantDto);

		Task<ServiceResult> DeleteAllApplicants();

		Task<ServiceResult> DeleteApplicant(string NationalId);

		Task<ServiceResult> EmployApplicant(string NationalId, EmployeeDto employeeDto);
	}
}
