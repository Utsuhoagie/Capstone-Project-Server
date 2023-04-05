using Capstone.Models;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using FluentValidation.Results;

namespace Capstone.Features.EmployeeModule
{
    public interface IEmployeeService
    {
		Task<PagedResult<EmployeeDto>> GetAllEmployees();

        Task<PagedResult<EmployeeDto>> GetEmployees(PagingParams pagingParams, EmployeeFilterParams filterParams);

		Task<EmployeeDto?> GetEmployee(string NationalId);

		Task<ServiceResult> AddEmployee(EmployeeDto employeeDto);

		Task<ServiceResult> UpdateEmployee(string NationalId, EmployeeDto employeeDto);

		Task<ServiceResult> DeleteAllEmployees();

		Task<ServiceResult> DeleteEmployee(string NationalId);
	}
}
