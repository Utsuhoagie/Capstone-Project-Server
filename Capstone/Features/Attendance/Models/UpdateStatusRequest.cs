using Capstone.Models;

namespace Capstone.Features.AttendanceModule.Models
{
	public class UpdateStatusRequest
	{
		public string EmployeeNationalId { get; set; } = string.Empty;
		public DateTimeOffset StartTimestamp { get; set; }
		public Status Status { get; set; }
	}
}
