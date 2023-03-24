using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using Capstone.Models;
using Capstone.Responses.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;

namespace Capstone.Features.PositionModule
{
    public class PositionService : IPositionService
    {
        private readonly CapstoneContext _context;
		private readonly IValidator<PositionDto> _validator;

		public PositionService(
			CapstoneContext capstoneContext, 
			IValidator<PositionDto> validator)
        {
            _context = capstoneContext;
			_validator = validator;
        }

		public async Task<PagedResult<PositionDto>> GetAllPositions()
		{
			var positions = await _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.Select(p => new PositionDto
				{
					Name = p.Name,
					ApplicantCount = p.Applicants.Count(),
					EmployeeCount = p.Employees.Count(),
				})
				.ToListAsync();

			var totalCount = await _context.Positions.CountAsync();

			return new PagedResult<PositionDto>(positions, totalCount, 1);
		}

		public async Task<PagedResult<PositionDto>> GetPositions(PagingParams pagingParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var queryableFilteredPositionDtos = _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.Select(p => new PositionDto
				{
					Name = p.Name,
					ApplicantCount = p.Applicants.Count(),
					EmployeeCount = p.Employees.Count(),
				});

			var pagedPositionDtos = await queryableFilteredPositionDtos
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredPositionDtos.CountAsync();

			return new PagedResult<PositionDto>(
				items: pagedPositionDtos,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<PositionDto?> GetPosition(string Name)
		{
			var position = await _context.Positions
				.Include(p => p.Applicants)
				.Include(p => p.Employees)
				.SingleOrDefaultAsync(p => p.Name == Name);

			if (position == null)
			{
				return null;
			}

			return new PositionDto
			{
				Name = position.Name,
				ApplicantCount = position.Applicants.Count(),
				EmployeeCount = position.Employees.Count(),
			};
		}

		public async Task<ServiceResult> AddPosition(PositionDto positionDto)
		{
			await _validator.ValidateAndThrowAsync(positionDto);

			var duplicatePosition = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == positionDto.Name);

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
				Name = positionDto.Name,
			};
			await _context.Positions.AddAsync(position);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdatePosition(string Name, PositionDto positionDto)
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
				.SingleOrDefaultAsync(p => p.Name == positionDto.Name);

			if (duplicatePosition != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePositionError
				};
			}

			position.Name = positionDto.Name;

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
