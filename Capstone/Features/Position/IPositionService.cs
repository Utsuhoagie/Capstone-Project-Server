using Capstone.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using FluentValidation.Results;

namespace Capstone.Features.PositionModule
{
    public interface IPositionService
    {
		Task<PagedResult<PositionDto>> GetAllPositions();

		Task<PagedResult<PositionDto>> GetPositions(PagingParams pagingParams);

		Task<PositionDto?> GetPosition(string Name);

		Task<ServiceResult> AddPosition(PositionDto applicantDto);

		Task<ServiceResult> UpdatePosition(string Name, PositionDto applicantDto);

		Task<ServiceResult> DeleteAllPositions();

		Task<ServiceResult> DeletePosition(string Name);
	}
}
