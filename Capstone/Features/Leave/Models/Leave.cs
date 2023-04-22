using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.LeaveModule.Models
{
	public class Leave
	{
		public int Id { get; set; }
		public DateTimeOffset StartDate { get; set; }
		public DateTimeOffset EndDate { get; set; }

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; } = default!;
	}

	public class LeaveRequest
	{
		public DateTimeOffset StartDate { get; set; }
		public DateTimeOffset EndDate { get; set; }
	}

	public class LeaveResponse
	{
		public DateTimeOffset StartDate { get; set; }
		public DateTimeOffset EndDate { get; set; }
		
		public string EmployeeNationalId { get; set; } = string.Empty;
	}
}
