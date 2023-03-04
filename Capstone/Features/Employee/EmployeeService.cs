using Capstone.Data;
using Capstone.ExceptionHandling;
using Capstone.Models;
using Capstone.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Capstone.Features.EmployeeModule
{
    public class EmployeeService : IEmployeeService
    {
        private readonly CapstoneContext _context;
		private readonly IValidator<EmployeeDto> _validator;

		public EmployeeService(
			CapstoneContext capstoneContext, 
			IValidator<EmployeeDto> validator)
        {
            _context = capstoneContext;
			_validator = validator;
        }

		public async Task<PagedResult<EmployeeDto>> GetAllEmployeesAsync()
		{
			var employeeDtos = await _context.People.OfType<Employee>()
				.Select(e => new EmployeeDto
					{
						NationalId = e.NationalId,
						FullName = e.FullName,
						Gender = e.Gender,
						BirthDate = e.BirthDate,
						Address = e.Address,
						Phone = e.Phone,
						Email = e.Email,
						ExperienceYears = e.ExperienceYears,
						Position = e.Position,
						Salary = e.Salary,
						EmployedDate = e.EmployedDate,
						StartHour = e.StartHour,
						EndHour = e.EndHour
					})
				.ToListAsync();

			var totalCount = await _context.People.OfType<Employee>().CountAsync();
			
			return new PagedResult<EmployeeDto>(employeeDtos, totalCount, 1);
		}

        public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
			PagingParams pagingParams,
			EmployeeFilterParams filterParams)
        {
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var SubName = filterParams.SubName;
			var Gender = filterParams.Gender;
			var Address = filterParams.Address;
			var ExperienceYears = filterParams.ExperienceYears;
			var Position = filterParams.Position;
			var EmployedDateFrom = filterParams.EmployedDateFrom;
			var EmployedDateTo = filterParams.EmployedDateTo;
			var Salary = filterParams.Salary;

			var queryableFilteredEmployeeDtos = _context.People.OfType<Employee>()
				.Where(e => SubName == null || e.FullName.ToLower().Contains(SubName.ToLower()))
				.Where(e => Gender == null || e.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(e => Address == null || e.Address.ToLower().Contains(Address.ToLower()))
				.Where(e => ExperienceYears == null || e.ExperienceYears == ExperienceYears)
				.Where(e => Position == null || e.Position == Position)
				.Where(e => ((EmployedDateFrom == null && EmployedDateTo == null) ||
					(e.EmployedDate >= EmployedDateFrom && e.EmployedDate <= EmployedDateTo)))
				.Where(e => Salary == null || e.Salary == Salary)
				.Select(e => new EmployeeDto
				{
					NationalId = e.NationalId,
					FullName = e.FullName,
					Gender = e.Gender,
					BirthDate = e.BirthDate,
					Address = e.Address,
					Phone = e.Phone,
					Email = e.Email,
					ExperienceYears = e.ExperienceYears,
					Position = e.Position,
					EmployedDate = e.EmployedDate,
					Salary = e.Salary,
					StartHour = e.StartHour,
					EndHour = e.EndHour
				});

			var pagedEmployeeDtos = await queryableFilteredEmployeeDtos
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredEmployeeDtos.CountAsync();

			return new PagedResult<EmployeeDto>(
				items: pagedEmployeeDtos, 
				totalCount: totalCount,
				page: page, 
				pageSize: pageSize);

        }

		public async Task<EmployeeDto?> GetEmployeeAsync(string NationalId)
		{
			var employee = await _context.People.OfType<Employee>()
				.SingleAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return null;
			}

			return new EmployeeDto
			{
				NationalId = employee.NationalId,
				FullName = employee.FullName,
				Gender = employee.Gender,
				BirthDate = employee.BirthDate,
				Address = employee.Address,
				Phone = employee.Phone,
				Email = employee.Email,
				ExperienceYears = employee.ExperienceYears,
				Position = employee.Position,
				Salary = employee.Salary,
				EmployedDate = employee.EmployedDate,
				StartHour = employee.StartHour,
				EndHour = employee.EndHour
			};
		}

		public async Task<bool> AddEmployeeAsync(EmployeeDto employeeDto)
		{
			await _validator.ValidateAndThrowAsync(employeeDto);

			//var duplicateEmployee = await _context.People.OfType<Employee>()
			//	.SingleAsync(e => e.NationalId == employeeDto.NationalId);

			//if (duplicateEmployee != null)
			//{
			//	return false;
			//}

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
				Position = employeeDto.Position,
				Salary = employeeDto.Salary,
				EmployedDate = employeeDto.EmployedDate,
				StartHour = employeeDto.StartHour,
				EndHour = employeeDto.EndHour
			};
			await _context.People.AddAsync(employee);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> UpdateEmployeeAsync(string NationalId, EmployeeDto employeeDto)
		{
			var employee = await _context.People.OfType<Employee>()
				.SingleAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return false;
			}

			employee.NationalId = employeeDto.NationalId;
			employee.FullName = employeeDto.FullName;
			employee.Gender = employeeDto.Gender;
			employee.BirthDate = employeeDto.BirthDate;
			employee.Address = employeeDto.Address;
			employee.Phone = employeeDto.Phone;
			employee.Email = employeeDto.Email;
			employee.ExperienceYears = employeeDto.ExperienceYears;
			employee.Position = employeeDto.Position;
			employee.Salary = employeeDto.Salary;
			employee.EmployedDate = employeeDto.EmployedDate;
			employee.StartHour = employeeDto.StartHour;
			employee.EndHour = employeeDto.EndHour;

			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteAllEmployeesAsync()
		{
			var employees = await _context.People.OfType<Employee>().ToListAsync();
			_context.People.RemoveRange(employees);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteEmployeeAsync(string NationalId)
		{
			var employee = await _context.People.OfType<Employee>()
				.SingleAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return false;
			}

			_context.People.Remove(employee);
			await _context.SaveChangesAsync();

			return true;
		}
	}
}
