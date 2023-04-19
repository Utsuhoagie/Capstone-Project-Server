using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using Capstone.Responses.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Capstone.Features.PositionModule.Models;

namespace Capstone.Features.PositionModule
{
	public interface IPositionService
	{
		Task<PagedResult<PositionResponse>> GetAllPositions();
		Task<PagedResult<PositionResponse>> GetPositions(PagingParams pagingParams);
		Task<PositionResponse?> GetPosition(string Name);
		Task<ServiceResult> AddPosition(PositionRequest req);
		Task<ServiceResult> UpdatePosition(string Name, PositionRequest req);
		Task<ServiceResult> DeleteAllPositions();
		Task<ServiceResult> DeletePosition(string Name);
	}

	public class PositionService : IPositionService
    {
        private readonly CapstoneContext _context;
		private readonly IValidator<PositionRequest> _validator;

		public PositionService(
			CapstoneContext capstoneContext, 
			IValidator<PositionRequest> validator)
        {
            _context = capstoneContext;
			_validator = validator;
        }

		public async Task<PagedResult<PositionResponse>> GetAllPositions()
		{
			var positions = await _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.Select(p => new PositionResponse
				{
					Name = p.Name,
					ApplicantCount = p.Applicants.Count(),
					EmployeeCount = p.Employees.Count(),
				})
				.ToListAsync();

			var totalCount = await _context.Positions.CountAsync();

			return new PagedResult<PositionResponse>(positions, totalCount, 1);
		}

		public async Task<PagedResult<PositionResponse>> GetPositions(PagingParams pagingParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var queryableFilteredPositionResponses = _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.Select(p => new PositionResponse
				{
					Name = p.Name,
					ApplicantCount = p.Applicants.Count(),
					EmployeeCount = p.Employees.Count(),
				});

			var pagedPositionResponses = await queryableFilteredPositionResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredPositionResponses.CountAsync();

			return new PagedResult<PositionResponse>(
				items: pagedPositionResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<PositionResponse?> GetPosition(string Name)
		{
			var position = await _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.SingleOrDefaultAsync(p => p.Name == Name);

			if (position == null)
			{
				return null;
			}

			return new PositionResponse
			{
				Name = position.Name,
				ApplicantCount = position.Applicants.Count(),
				EmployeeCount = position.Employees.Count(),
			};
		}

		public async Task<ServiceResult> AddPosition(PositionRequest req)
		{
			await _validator.ValidateAndThrowAsync(req);

			var duplicatePosition = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == req.Name);

			if (duplicatePosition != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePositionError
				};
			}

			var position = new Position
			{
				Name = req.Name,
			};
			await _context.Positions.AddAsync(position);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdatePosition(string Name, PositionRequest req)
		{
			var position = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == Name);

			if (position == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			var duplicatePosition = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == req.Name);

			if (duplicatePosition != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePositionError
				};
			}

			position.Name = req.Name;

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> DeleteAllPositions()
		{
			var positions = await _context.Positions.ToListAsync();
			_context.Positions.RemoveRange(positions);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> DeletePosition(string Name)
		{
			var position = await _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.SingleOrDefaultAsync(p => p.Name == Name);

			if (position == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			if (position.Applicants.Count > 0 || position.Employees.Count > 0)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.PositionHasPeopleError
				};
			}

			_context.Positions.Remove(position);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}
	}
}
