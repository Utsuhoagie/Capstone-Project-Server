using Capstone.Features.AttendanceModule.Models;

namespace Capstone.Features.AttendanceModule.Models
{
    public class UpdateStatusRequest
	{
		public string EmployeeNationalId { get; set; } = string.Empty;
		public DateTimeOffset StartTimestamp { get; set; }
		public AttendanceStatus Status { get; set; }
	}
}
