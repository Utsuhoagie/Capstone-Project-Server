using Capstone.Models;
using Capstone.Pagination;
using FluentValidation.Results;

namespace Capstone.Features.EmployeeModule
{
    public interface IEmployeeService
    {
		Task<PagedResult<EmployeeDto>> GetAllEmployeesAsync();

        Task<PagedResult<EmployeeDto>> GetEmployeesAsync(PagingParams pagingParams, EmployeeFilterParams filterParams);

		Task<EmployeeDto?> GetEmployeeAsync(string NationalId);

		Task<bool> AddEmployeeAsync(EmployeeDto employeeDto);

		Task<bool> UpdateEmployeeAsync(string NationalId, EmployeeDto employeeDto);

		Task<bool> DeleteAllEmployeesAsync();

		Task<bool> DeleteEmployeeAsync(string NationalId);
	}
}
