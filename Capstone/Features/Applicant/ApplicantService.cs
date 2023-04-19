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
using Capstone.Features.ApplicantModule.Models;
using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.ApplicantModule
{
	public interface IApplicantService
	{
		Task<PagedResult<ApplicantResponse>> GetAllApplicants();
		Task<PagedResult<ApplicantResponse>> GetApplicants(PagingParams pagingParams, ApplicantFilterParams filterParams);
		Task<ApplicantResponse?> GetApplicant(string NationalId);
		Task<ServiceResult> AddApplicant(ApplicantRequest req);
		Task<ServiceResult> UpdateApplicant(string NationalId, ApplicantRequest req);
		Task<ServiceResult> DeleteAllApplicants();
		Task<ServiceResult> DeleteApplicant(string NationalId);
		Task<ServiceResult> EmployApplicant(string NationalId, EmployeeRequest req);
	}
	public class ApplicantService : IApplicantService
	{
		private readonly CapstoneContext _context;
		private readonly IValidator<ApplicantRequest> _validator;
		private readonly string DANGEROUS_FILE_PATH;

		public ApplicantService(
			CapstoneContext capstoneContext,
			IValidator<ApplicantRequest> validator,
			IConfiguration configuration)
		{
			_context = capstoneContext;
			_validator = validator;
			DANGEROUS_FILE_PATH = $"{configuration.GetSection("FilePath").Value}\\Applicants";
		}

		public async Task<PagedResult<ApplicantResponse>> GetAllApplicants()
		{
			var applicants = await _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.Select(a => new ApplicantResponse
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

			return new PagedResult<ApplicantResponse>(applicants, totalCount, 1);
		}

		public async Task<PagedResult<ApplicantResponse>> GetApplicants(
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

			var queryableFilteredApplicantResponses = _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.Where(a => SubName == null || a.FullName.ToLower().Contains(SubName.ToLower()))
				.Where(a => Gender == null || a.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(a => Address == null || a.Address.ToLower().Contains(Address.ToLower()))
				.Where(a => ExperienceYears == null || a.ExperienceYears == ExperienceYears)
				.Where(a => AppliedPositionName == null || a.AppliedPosition.Name == AppliedPositionName)
				.Where(a => ((AppliedDateFrom == null && AppliedDateTo == null) ||
					(a.AppliedDate >= AppliedDateFrom && a.AppliedDate <= AppliedDateTo)))
				.Where(a => AskingSalary == null || a.AskingSalary == AskingSalary)
				.Select(a => new ApplicantResponse
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
					ImageFileName = a.ImageFileName,
				});

			var pagedApplicantResponses = await queryableFilteredApplicantResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredApplicantResponses.CountAsync();

			return new PagedResult<ApplicantResponse>(
				items: pagedApplicantResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<ApplicantResponse?> GetApplicant(string NationalId)
		{
			var applicant = await _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)
				.SingleOrDefaultAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return null;
			}

			return new ApplicantResponse
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
				ImageFileName = applicant.ImageFileName,
			};
		}

		public async Task<ServiceResult> AddApplicant(ApplicantRequest req)
		{
			await _validator.ValidateAndThrowAsync(req);

			var duplicatePerson = await _context.People
				.SingleOrDefaultAsync(p => p.NationalId == req.NationalId);

			if (duplicatePerson != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePersonError
				};
			}

			var appliedPosition = await _context.Positions
				.SingleOrDefaultAsync(p => p.Name == req.AppliedPositionName);

			if (appliedPosition == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			// Upload file
			var safeFileName = $"{req.NationalId}";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, safeFileName);
			var safeFilePathNameWithCorrectExtension = Path.ChangeExtension(safeFilePathName, "jpeg");
			using (var fileStream = System.IO.File.Create(safeFilePathNameWithCorrectExtension))
			{
				if (req.Image != null)
				{
					await req.Image.CopyToAsync(fileStream);
				}
			}

			var applicant = new Applicant
			{
				NationalId = req.NationalId,
				FullName = req.FullName,
				Gender = req.Gender,
				BirthDate = req.BirthDate,
				Address = req.Address,
				Phone = req.Phone,
				Email = req.Email,
				ExperienceYears = req.ExperienceYears,
				AppliedPosition = appliedPosition,
				AppliedDate = req.AppliedDate,
				AskingSalary = req.AskingSalary,
				ImageFileName = req.Image != null ?
					Path.GetFileName(safeFilePathNameWithCorrectExtension) :
					null,
			};
			await _context.People.AddAsync(applicant);
			appliedPosition.Applicants.Add(applicant);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdateApplicant(string NationalId, ApplicantRequest req)
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
				.SingleOrDefaultAsync(p => p.Name == req.AppliedPositionName);

			if (newPosition == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoPositionError
				};
			}

			applicant.NationalId = req.NationalId;
			applicant.FullName = req.FullName;
			applicant.Gender = req.Gender;
			applicant.BirthDate = req.BirthDate;
			applicant.Address = req.Address;
			applicant.Phone = req.Phone;
			applicant.Email = req.Email;
			applicant.ExperienceYears = req.ExperienceYears;
			applicant.AppliedPosition = newPosition;
			applicant.AppliedDate = req.AppliedDate;
			applicant.AskingSalary = req.AskingSalary;

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

		public async Task<ServiceResult> EmployApplicant(string NationalId, EmployeeRequest req)
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
				.SingleOrDefaultAsync(p => p.Name == req.PositionName);

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
				NationalId = req.NationalId,
				FullName = req.FullName,
				Gender = req.Gender,
				BirthDate = req.BirthDate,
				Address = req.Address,
				Phone = req.Phone,
				Email = req.Email,
				ExperienceYears = req.ExperienceYears,
				Position = position,
				EmployedDate = req.EmployedDate,
				Salary = req.Salary,
				StartHour = req.StartHour,
				EndHour = req.EndHour,
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
