using Capstone.Models;
using Capstone.Pagination;
using FluentValidation.Results;

namespace Capstone.Features.ApplicantTracking
{
    public interface IApplicantTrackingService
    {
		Task<PagedResult<ApplicantDto>> GetAllApplicantsAsync();

        Task<PagedResult<ApplicantDto>> GetApplicantsAsync(PagingParams pagingParams, ApplicantTrackingFilterParams filterParams);

		Task<ApplicantDto?> GetApplicantAsync(string NationalId);

		Task<bool> AddApplicantAsync(ApplicantDto applicantDto);

		Task<bool> UpdateApplicantAsync(string NationalId, ApplicantDto applicantDto);

		Task<bool> DeleteApplicantsAsync();

		Task<bool> DeleteApplicantAsync(string NationalId);
	}
}
