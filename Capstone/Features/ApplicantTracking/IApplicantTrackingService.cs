using Capstone.Models;
using FluentValidation.Results;

namespace Capstone.Features.ApplicantTracking
{
    public interface IApplicantTrackingService
    {
        Task<IEnumerable<ApplicantDto>> GetApplicantsAsync();

		Task<ApplicantDto?> GetApplicantAsync(int id);

		Task<bool> AddApplicantAsync(ApplicantDto applicantDto);

		Task<bool> DeleteApplicantsAsync();

		Task<bool> DeleteApplicantAsync(string NationalId);
	}
}
