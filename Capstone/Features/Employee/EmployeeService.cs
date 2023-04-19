﻿using Capstone.Data;
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

namespace Capstone.Features.EmployeeModule
{
	public interface IEmployeeService
	{
		Task<PagedResult<EmployeeResponse>> GetAllEmployees();
		Task<PagedResult<EmployeeResponse>> GetEmployees(PagingParams pagingParams, EmployeeFilterParams filterParams);
		Task<EmployeeResponse?> GetEmployee(string NationalId);
		Task<ServiceResult> AddEmployee(EmployeeRequest req);
		Task<ServiceResult> UpdateEmployee(string NationalId, EmployeeRequest req);
		Task<ServiceResult> DeleteAllEmployees();
		Task<ServiceResult> DeleteEmployee(string NationalId);
	}
	public class EmployeeService : IEmployeeService
	{
		private readonly CapstoneContext _context;
		private readonly IValidator<EmployeeRequest> _validator;

		public EmployeeService(
			CapstoneContext capstoneContext,
			IValidator<EmployeeRequest> validator)
		{
			_context = capstoneContext;
			_validator = validator;
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
					StartHour = e.StartHour,
					EndHour = e.EndHour,
					HasUser = e.User != null,
				})
				.ToListAsync();

			var totalCount = await _context.People.OfType<Employee>().CountAsync();

			return new PagedResult<EmployeeResponse>(employeeResponses, totalCount, 1);
		}

		public async Task<PagedResult<EmployeeResponse>> GetEmployees(
			PagingParams pagingParams,
			EmployeeFilterParams filterParams)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var SubName = filterParams.SubName;
			var Gender = filterParams.Gender;
			var Address = filterParams.Address;
			var ExperienceYears = filterParams.ExperienceYears;
			var PositionName = filterParams.PositionName;
			var EmployedDateFrom = filterParams.EmployedDateFrom;
			var EmployedDateTo = filterParams.EmployedDateTo;
			var Salary = filterParams.Salary;

			var queryableFilteredEmployeeDtos = _context.People.OfType<Employee>()
				.Include(e => e.User)
				.Include(e => e.Position)
				.Where(e => SubName == null || e.FullName.ToLower().Contains(SubName.ToLower()))
				.Where(e => Gender == null || e.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(e => Address == null || e.Address.ToLower().Contains(Address.ToLower()))
				.Where(e => ExperienceYears == null || e.ExperienceYears == ExperienceYears)
				.Where(e => PositionName == null || e.Position.Name == PositionName)
				.Where(e => ((EmployedDateFrom == null && EmployedDateTo == null) ||
					(e.EmployedDate >= EmployedDateFrom && e.EmployedDate <= EmployedDateTo)))
				.Where(e => Salary == null || e.Salary == Salary)
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
					StartHour = e.StartHour,
					EndHour = e.EndHour,
					HasUser = e.User != null,
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
				StartHour = employee.StartHour,
				EndHour = employee.EndHour,
				HasUser = employee.User != null,
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
				StartHour = req.StartHour,
				EndHour = req.EndHour,
				User = null,
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
			employee.StartHour = req.StartHour;
			employee.EndHour = req.EndHour;
			//employee.User = employee.User;

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
