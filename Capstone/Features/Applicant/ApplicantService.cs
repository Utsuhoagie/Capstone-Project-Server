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

namespace Capstone.Features.ApplicantModule
{
	public class ApplicantService : IApplicantService
	{
		private readonly CapstoneContext _context;
		private readonly IValidator<ApplicantDto> _validator;

		public ApplicantService(
			CapstoneContext capstoneContext,
			IValidator<ApplicantDto> validator)
		{
			_context = capstoneContext;
			_validator = validator;
		}

		public async Task<PagedResult<ApplicantDto>> GetAllApplicants()
		{
			var applicants = await _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.Select(a => new ApplicantDto
				{
					NationalId = a.NationalId,
					FullName = a.FullName,
					Gender = a.Gender,
					BirthDate = a.BirthDate,
					Address = a.Address,
					Phone = a.Phone,
					Email = a.Email,
					ExperienceYears = a.ExperienceYears,
					AppliedPositionName = a.AppliedPosition.Name,
					AppliedDate = a.AppliedDate,
					AskingSalary = a.AskingSalary,
				})
				.ToListAsync();

			var totalCount = await _context.People.OfType<Applicant>().CountAsync();

			return new PagedResult<ApplicantDto>(applicants, totalCount, 1);
		}

		public async Task<PagedResult<ApplicantDto>> GetApplicants(
			PagingParams pagingParams,
			ApplicantFilterParams filterParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var SubName = filterParams.SubName;
			var Gender = filterParams.Gender;
			var Address = filterParams.Address;
			var ExperienceYears = filterParams.ExperienceYears;
			var AppliedPositionName = filterParams.AppliedPositionName;
			var AppliedDateFrom = filterParams.AppliedDateFrom;
			var AppliedDateTo = filterParams.AppliedDateTo;
			var AskingSalary = filterParams.AskingSalary;

			var queryableFilteredApplicantDtos = _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.Where(a => SubName == null || a.FullName.ToLower().Contains(SubName.ToLower()))
				.Where(a => Gender == null || a.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(a => Address == null || a.Address.ToLower().Contains(Address.ToLower()))
				.Where(a => ExperienceYears == null || a.ExperienceYears == ExperienceYears)
				.Where(a => AppliedPositionName == null || a.AppliedPosition.Name == AppliedPositionName)
				.Where(a => ((AppliedDateFrom == null && AppliedDateTo == null) ||
					(a.AppliedDate >= AppliedDateFrom && a.AppliedDate <= AppliedDateTo)))
				.Where(a => AskingSalary == null || a.AskingSalary == AskingSalary)
				.Select(a => new ApplicantDto
				{
					NationalId = a.NationalId,
					FullName = a.FullName,
					Gender = a.Gender,
					BirthDate = a.BirthDate,
					Address = a.Address,
					Phone = a.Phone,
					Email = a.Email,
					ExperienceYears = a.ExperienceYears,
					AppliedPositionName = a.AppliedPosition.Name,
					AppliedDate = a.AppliedDate,
					AskingSalary = a.AskingSalary,
				});

			var pagedApplicantDtos = await queryableFilteredApplicantDtos
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredApplicantDtos.CountAsync();

			return new PagedResult<ApplicantDto>(
				items: pagedApplicantDtos,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);

		}

		public async Task<ApplicantDto?> GetApplicant(string NationalId)
		{
			var applicant = await _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.SingleOrDefaultAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return null;
			}

			return new ApplicantDto
			{
				NationalId = applicant.NationalId,
				FullName = applicant.FullName,
				Gender = applicant.Gender,
				BirthDate = applicant.BirthDate,
				Address = applicant.Address,
				Phone = applicant.Phone,
				Email = applicant.Email,
				ExperienceYears = applicant.ExperienceYears,
				AppliedPositionName = applicant.AppliedPosition.Name,
				AppliedDate = applicant.AppliedDate,
				AskingSalary = applicant.AskingSalary,
			};
		}

		public async Task<ServiceResult> AddApplicant(ApplicantDto applicantDto)
		{
			await _validator.ValidateAndThrowAsync(applicantDto);

			var duplicatePerson = await _context.People
				.SingleOrDefaultAsync(p => p.NationalId == applicantDto.NationalId);

			if (duplicatePerson != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePersonError
				};
			}

			var appliedPosition = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == applicantDto.AppliedPositionName);

			if (appliedPosition == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			var applicant = new Applicant
			{
				NationalId = applicantDto.NationalId,
				FullName = applicantDto.FullName,
				Gender = applicantDto.Gender,
				BirthDate = applicantDto.BirthDate,
				Address = applicantDto.Address,
				Phone = applicantDto.Phone,
				Email = applicantDto.Email,
				ExperienceYears = applicantDto.ExperienceYears,
				AppliedPosition = appliedPosition,
				AppliedDate = applicantDto.AppliedDate,
				AskingSalary = applicantDto.AskingSalary,
			};
			await _context.People.AddAsync(applicant);
			appliedPosition.Applicants.Add(applicant);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdateApplicant(string NationalId, ApplicantDto applicantDto)
		{
			var applicant = await _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.SingleOrDefaultAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoApplicantError
				};
			}

			var oldPosition = applicant.AppliedPosition;

			var newPosition = await _context.Positions
				.Include(p => p.Applicants)
				.SingleOrDefaultAsync(p => p.Name == applicantDto.AppliedPositionName);

			if (newPosition == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			applicant.NationalId = applicantDto.NationalId;
			applicant.FullName = applicantDto.FullName;
			applicant.Gender = applicantDto.Gender;
			applicant.BirthDate = applicantDto.BirthDate;
			applicant.Address = applicantDto.Address;
			applicant.Phone = applicantDto.Phone;
			applicant.Email = applicantDto.Email;
			applicant.ExperienceYears = applicantDto.ExperienceYears;
			applicant.AppliedPosition = newPosition;
			applicant.AppliedDate = applicantDto.AppliedDate;
			applicant.AskingSalary = applicantDto.AskingSalary;

			oldPosition.Applicants.Remove(applicant);
			newPosition.Applicants.Add(applicant);

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> DeleteAllApplicants()
		{
			var applicants = await _context.People.OfType<Applicant>().ToListAsync();
			_context.People.RemoveRange(applicants);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> DeleteApplicant(string NationalId)
		{
			var applicant = await _context.People.OfType<Applicant>()
				.SingleOrDefaultAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoApplicantError
				};
			}

			_context.People.Remove(applicant);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> EmployApplicant(string NationalId, EmployeeDto employeeDto)
		{
			var applicant = await _context.People.OfType<Applicant>()
				.SingleOrDefaultAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoApplicantError
				};
			}

			var position = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == employeeDto.PositionName);

			if (position == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			var employee = new Employee
			{
				NationalId = employeeDto.NationalId,
				FullName = employeeDto.FullName,
				Gender = employeeDto.Gender,
				BirthDate = employeeDto.BirthDate,
				Address = employeeDto.Address,
				Phone = employeeDto.Phone,
				Email = employeeDto.Email,
				ExperienceYears = employeeDto.ExperienceYears,
				Position = position,
				EmployedDate = employeeDto.EmployedDate,
				Salary = employeeDto.Salary,
				StartHour = employeeDto.StartHour,
				EndHour = employeeDto.EndHour,
				User = null,
			};

			_context.People.Remove(applicant);
			_context.People.Add(employee);

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}
	}
}
