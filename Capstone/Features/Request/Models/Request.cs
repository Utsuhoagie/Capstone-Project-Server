using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.RequestModule.Models
{
	public enum RequestStatus
	{
		Pending = 0,
		Accepted = 1,
		Rejected = -1,
	}
	public class Request
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public DateTimeOffset CreatedDate { get; set; }
		public RequestStatus RequestStatus { get; set; }

		public int? NewSalary { get; set; }
		public DateTimeOffset? StartLeaveDate { get; set; }
		public DateTimeOffset? EndLeaveDate { get; set; }

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; } = new Employee();
	}
	public class UpdateRequestStatusRequest
	{
		public int Id { get; set; }
		public RequestStatus RequestStatus { get; set; }
	}
	public class AddRequestRequest
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;

		public int? NewSalary { get; set; }
		public DateTimeOffset? StartLeaveDate { get; set; }
		public DateTimeOffset? EndLeaveDate { get; set; }

		public string EmployeeNationalId { get; set; } = string.Empty;
	}
	public class RequestResponse
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public DateTimeOffset CreatedDate { get; set; }
		public RequestStatus RequestStatus { get; set; }

		public int? NewSalary { get; set; }
		public DateTimeOffset? StartLeaveDate { get; set; }
		public DateTimeOffset? EndLeaveDate { get; set; }

		public string EmployeeFullName { get; set; } = string.Empty;
	}
}
