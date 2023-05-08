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
using Capstone.Features.EmployeeModule.Models;
using Microsoft.AspNetCore.Identity;
using Capstone.Features.Auth.Models;
using System.Configuration;

namespace Capstone.Features.EmployeeModule
{
    public interface IEmployeeService
	{
		Task<PagedResult<EmployeeResponse>> GetAllEmployees();
		Task<PagedResult<EmployeeResponse>> GetEmployees(PagingParams pagingParams, EmployeeParams employeeParams);
		Task<EmployeeResponse?> GetEmployee(string NationalId);
		Task<ServiceResult> AddEmployee(EmployeeRequest req);
		Task<ServiceResult> UpdateEmployee(string NationalId, EmployeeRequest req);
		Task<ServiceResult> UpdateSelf(string NationalId, UpdateSelfRequest req);
		Task<ServiceResult> DeleteAllEmployees();
		Task<ServiceResult> DeleteEmployee(string NationalId);
	}
	public class EmployeeService : IEmployeeService
	{
		private readonly CapstoneContext _context;
		private readonly IValidator<EmployeeRequest> _validator;
		private readonly UserManager<EmployeeUser> _userManager;
		private readonly string DANGEROUS_FILE_PATH;

		public EmployeeService(
			CapstoneContext capstoneContext,
			IValidator<EmployeeRequest> validator, 
			UserManager<EmployeeUser> userManager,
			IConfiguration configuration)
		{
			_context = capstoneContext;
			_validator = validator;
			_userManager = userManager;
			DANGEROUS_FILE_PATH = $"{configuration.GetSection("FilePath").Value}\\Employees";
		}

		public async Task<PagedResult<EmployeeResponse>> GetAllEmployees()
		{
			var employeeResponses = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.Include(e => e.Position)
				.Select(e => new EmployeeResponse
				{
					NationalId = e.NationalId,
					FullName = e.FullName,
					Gender = e.Gender,
					BirthDate = e.BirthDate,
					Address = e.Address,
					Phone = e.Phone,
					Email = e.Email,
					ExperienceYears = e.ExperienceYears,
					PositionName = e.Position.Name,
					Salary = e.Salary,
					EmployedDate = e.EmployedDate,
					HasUser = e.User != null,
					ImageFileName = e.ImageFileName,
					ResumeFileName = e.ResumeFileName,
				})
				.ToListAsync();

			var totalCount = await _context.People.OfType<Employee>().CountAsync();

			return new PagedResult<EmployeeResponse>(employeeResponses, totalCount, 1);
		}

		public async Task<PagedResult<EmployeeResponse>> GetEmployees(
			PagingParams pagingParams,
			EmployeeParams employeeParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var NamePart = employeeParams.NamePart;
			var Gender = employeeParams.Gender;
			var Address = employeeParams.Address;
			var PositionName = employeeParams.PositionName;
			var ExperienceYearsFrom = employeeParams.ExperienceYearsFrom;
			var ExperienceYearsTo = employeeParams.ExperienceYearsTo;
			var EmployedDateFrom = employeeParams.EmployedDateFrom;
			var EmployedDateTo = employeeParams.EmployedDateTo;
			var SalaryFrom = employeeParams.SalaryFrom;
			var SalaryTo = employeeParams.SalaryTo;

			var queryableFilteredEmployeeDtos = _context.People.OfType<Employee>()
				.Include(e => e.User)
				.Include(e => e.Position)
				.Where(e => NamePart == null || e.FullName.ToLower().Contains(NamePart.ToLower()))
				.Where(e => Gender == null || e.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(e => Address == null || e.Address.ToLower().Contains(Address.ToLower()))
				.Where(e => PositionName == null || e.Position.Name == PositionName)

				.Where(e => ExperienceYearsFrom == null || e.ExperienceYears >= ExperienceYearsFrom)
				.Where(e => ExperienceYearsTo == null || e.ExperienceYears <= ExperienceYearsTo)
				
				.Where(e => EmployedDateFrom == null || e.EmployedDate.Date >= EmployedDateFrom.Value.Date)
				.Where(e => EmployedDateTo == null || e.EmployedDate.Date <= EmployedDateTo.Value.Date)

				.Where(e => SalaryFrom == null || e.Salary >= SalaryFrom)
				.Where(e => SalaryTo == null || e.Salary <= SalaryTo)

				.Select(e => new EmployeeResponse
				{
					NationalId = e.NationalId,
					FullName = e.FullName,
					Gender = e.Gender,
					BirthDate = e.BirthDate,
					Address = e.Address,
					Phone = e.Phone,
					Email = e.Email,
					ExperienceYears = e.ExperienceYears,
					PositionName = e.Position.Name,
					EmployedDate = e.EmployedDate,
					Salary = e.Salary,
					HasUser = e.User != null,
					ImageFileName = e.ImageFileName,
					ResumeFileName = e.ResumeFileName,
				});

			var pagedEmployeeDtos = await queryableFilteredEmployeeDtos
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredEmployeeDtos.CountAsync();

			return new PagedResult<EmployeeResponse>(
				items: pagedEmployeeDtos,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<EmployeeResponse?> GetEmployee(string NationalId)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.Include(e => e.Position)
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return null;
			}

			return new EmployeeResponse
			{
				NationalId = employee.NationalId,
				FullName = employee.FullName,
				Gender = employee.Gender,
				BirthDate = employee.BirthDate,
				Address = employee.Address,
				Phone = employee.Phone,
				Email = employee.Email,
				ExperienceYears = employee.ExperienceYears,
				PositionName = employee.Position.Name,
				Salary = employee.Salary,
				EmployedDate = employee.EmployedDate,
				HasUser = employee.User != null,
				ImageFileName = employee.ImageFileName,
				ResumeFileName = employee.ResumeFileName,
			};
		}

		public async Task<ServiceResult> AddEmployee(EmployeeRequest req)
		{
			await _validator.ValidateAndThrowAsync(req);

			var duplicatePerson = await _context.People
				.SingleOrDefaultAsync(p => p.NationalId == req.NationalId);

			if (duplicatePerson != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.DuplicatePersonError,
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

			// Upload file
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
				Salary = req.Salary,
				EmployedDate = req.EmployedDate,
				User = null,
				ImageFileName = req.Image != null? 
					Path.GetFileName(imageFilePathName) : 
					null,
				ResumeFileName = req.Resume != null? 
					Path.GetFileName(resumeFilePathName) : 
					null,
			};

			await _context.People.AddAsync(employee);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdateEmployee(string NationalId, EmployeeRequest req)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
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

			// Upload file
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

			employee.NationalId = req.NationalId;
			employee.FullName = req.FullName;
			employee.Gender = req.Gender;
			employee.BirthDate = req.BirthDate;
			employee.Address = req.Address;
			employee.Phone = req.Phone;
			employee.Email = req.Email;
			employee.ExperienceYears = req.ExperienceYears;
			employee.Position = position;
			employee.Salary = req.Salary;
			employee.EmployedDate = req.EmployedDate;
			//employee.User = employee.User;
			employee.ImageFileName = req.Image != null ?
				Path.GetFileName(imageFilePathName) :
				null;
			employee.ResumeFileName = req.Resume != null ?
				Path.GetFileName(resumeFilePathName) :
				null;

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdateSelf(string NationalId, UpdateSelfRequest req)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
				};
			}

			if (NationalId != req.NationalId)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.EmployeeNotSelfError,
				};
			}

			// Upload file
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

			//employee.NationalId = req.NationalId;
			employee.BirthDate = req.BirthDate != null? req.BirthDate : employee.BirthDate;
			employee.Address = req.Address != null? req.Address : employee.Address;
			employee.Phone = req.Phone != null? req.Phone : employee.Phone;
			employee.ImageFileName = req.Image != null ?
				Path.GetFileName(imageFilePathName) :
				employee.ImageFileName;
			employee.ResumeFileName = req.Resume != null ?
				Path.GetFileName(resumeFilePathName) :
				employee.ResumeFileName;

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}


		public async Task<ServiceResult> DeleteAllEmployees()
		{
			var employees = await _context.People.OfType<Employee>().ToListAsync();
			_context.People.RemoveRange(employees);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> DeleteEmployee(string NationalId)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.User)
				.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
				};
			}

			_context.People.Remove(employee);
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}
	}
}
