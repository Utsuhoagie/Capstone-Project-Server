using Capstone.Data;
using Capstone.Responses.ExceptionHandling;
using Capstone.Responses.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.IO;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Capstone.Features.ApplicantModule.Models;
using Capstone.Features.EmployeeModule.Models;
using System.Diagnostics;

namespace Capstone.Features.ApplicantModule
{
    public interface IApplicantService
	{
		Task<PagedResult<ApplicantResponse>> GetAllApplicants();
		Task<PagedResult<ApplicantResponse>> GetApplicants(PagingParams pagingParams, ApplicantParams applicantParams);
		Task<ApplicantResponse?> GetApplicant(string NationalId);
		Task<ServiceResult> AddApplicant(ApplicantRequest req);
		Task<ServiceResult> UpdateApplicant(string NationalId, ApplicantRequest req);
		Task<ServiceResult> EmployApplicant(string NationalId, EmployeeRequest req);
		Task<ServiceResult> DeleteAllApplicants();
		Task<ServiceResult> DeleteApplicant(string NationalId);
	}
	public class ApplicantService : IApplicantService
	{
		private readonly CapstoneContext _context;
		private readonly IValidator<ApplicantRequest> _validator;
		private readonly string DANGEROUS_FILE_PATH;
		private readonly string DANGEROUS_EMPLOYEE_FILE_PATH;

		public ApplicantService(
			CapstoneContext capstoneContext,
			IValidator<ApplicantRequest> validator,
			IConfiguration configuration)
		{
			_context = capstoneContext;
			_validator = validator;
			DANGEROUS_FILE_PATH = $"{configuration.GetSection("FilePath").Value}\\Applicants";
			DANGEROUS_EMPLOYEE_FILE_PATH = $"{configuration.GetSection("FilePath").Value}\\Employees";
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
					ImageFileName = a.ImageFileName,
					ResumeFileName = a.ResumeFileName,
				})
				.ToListAsync();

			var totalCount = await _context.People.OfType<Applicant>().CountAsync();

			return new PagedResult<ApplicantResponse>(applicants, totalCount, 1);
		}

		public async Task<PagedResult<ApplicantResponse>> GetApplicants(
			PagingParams pagingParams,
			ApplicantParams applicantParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var NamePart = applicantParams.NamePart;
			var Gender = applicantParams.Gender;
			var Address = applicantParams.Address;
			var AppliedPositionName = applicantParams.AppliedPositionName;

			var ExperienceYearsFrom = applicantParams.ExperienceYearsFrom;
			var ExperienceYearsTo = applicantParams.ExperienceYearsTo;
			var AppliedDateFrom = applicantParams.AppliedDateFrom;
			var AppliedDateTo = applicantParams.AppliedDateTo;
			var AskingSalaryFrom = applicantParams.AskingSalaryFrom;
			var AskingSalaryTo = applicantParams.AskingSalaryTo;

			var queryableFilteredApplicantResponses = _context.People.OfType<Applicant>()
				.Include(a => a.AppliedPosition)

				.Where(a => NamePart == null || a.FullName.ToLower().Contains(NamePart.ToLower()))
				.Where(a => Gender == null || a.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(a => Address == null || a.Address.ToLower().Contains(Address.ToLower()))
				.Where(a => AppliedPositionName == null || a.AppliedPosition.Name == AppliedPositionName)

				.Where(a => ExperienceYearsFrom == null || a.ExperienceYears >= ExperienceYearsFrom)
				.Where(a => ExperienceYearsTo == null || a.ExperienceYears <= ExperienceYearsTo)
				
				.Where(a => AppliedDateFrom == null || a.AppliedDate.Date >= AppliedDateFrom.Value.Date)
				.Where(a => AppliedDateTo == null || a.AppliedDate.Date <= AppliedDateTo.Value.Date)
				
				.Where(a => AskingSalaryFrom == null || a.AskingSalary >= AskingSalaryFrom)
				.Where(a => AskingSalaryTo == null || a.AskingSalary <= AskingSalaryTo)
				
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
					ResumeFileName = a.ResumeFileName,
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
				ResumeFileName = applicant.ResumeFileName,
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

			// Upload files
			var imageFilePathName = Path.ChangeExtension(
				Path.Combine(DANGEROUS_FILE_PATH, $"{req.NationalId}"), 
				"jpeg");
			var resumeFilePathName = Path.ChangeExtension(
				Path.Combine(DANGEROUS_FILE_PATH, $"{req.NationalId}"), 
				"pdf");
			if (req.Image != null)
			{
				using (var fileStream = File.Create(imageFilePathName))
				{
					await req.Image.CopyToAsync(fileStream);
				}
			}
			if (req.Resume != null)
			{
				using (var fileStream = File.Create(resumeFilePathName))
				{
					await req.Resume.CopyToAsync(fileStream);
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
					Path.GetFileName(imageFilePathName) :
					null,
				ResumeFileName = req.Resume != null ?
					Path.GetFileName(resumeFilePathName) : 
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

			// Upload files
			var imageFilePathName = Path.ChangeExtension(
				Path.Combine(DANGEROUS_FILE_PATH, $"{req.NationalId}"),
				"jpeg");
			var resumeFilePathName = Path.ChangeExtension(
				Path.Combine(DANGEROUS_FILE_PATH, $"{req.NationalId}"),
				"pdf");
			if (req.Image != null)
			{
				using (var fileStream = File.Create(imageFilePathName))
				{
					await req.Image.CopyToAsync(fileStream);
				}
			}
			if (req.Resume != null)
			{
				using (var fileStream = File.Create(resumeFilePathName))
				{
					await req.Resume.CopyToAsync(fileStream);
				}
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
			applicant.ImageFileName = req.Image != null? 
				Path.GetFileName(imageFilePathName) :
				null;
			applicant.ResumeFileName = req.Resume != null? 
				Path.GetFileName(resumeFilePathName) :
				null;

			oldPosition.Applicants.Remove(applicant);
			newPosition.Applicants.Add(applicant);

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

			// Delete Applicant image/resume
			if (applicant.ImageFileName != null)
			{
				File.Delete(Path.Combine(DANGEROUS_FILE_PATH, applicant.ImageFileName));
			}
			if (applicant.ResumeFileName != null)
			{
				File.Delete(Path.Combine(DANGEROUS_FILE_PATH, applicant.ResumeFileName));
			}

			//...then add Employee image/resume
			var imageFilePathName_Employee = Path.ChangeExtension(
				Path.Combine(DANGEROUS_EMPLOYEE_FILE_PATH, $"{req.NationalId}"),
				"jpeg");
			var resumeFilePathName_Employee = Path.ChangeExtension(
				Path.Combine(DANGEROUS_EMPLOYEE_FILE_PATH, $"{req.NationalId}"),
				"pdf");
			if (req.Image != null)
			{
				using (var fileStream = File.Create(imageFilePathName_Employee))
				{
					await req.Image.CopyToAsync(fileStream);
				}
			}
			if (req.Resume != null)
			{
				using (var fileStream = File.Create(resumeFilePathName_Employee))
				{
					await req.Resume.CopyToAsync(fileStream);
				}
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
				User = null,
				ImageFileName = req.Image != null ?
					Path.GetFileName(imageFilePathName_Employee) :
					null,
				ResumeFileName = req.Resume != null ?
					Path.GetFileName(resumeFilePathName_Employee) :
					null,
			};

			_context.People.Remove(applicant);
			_context.People.Add(employee);

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
	}
}
