using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.AttendanceModule.Models
{
    public enum AttendanceStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = -1
    }

    public class Attendance
    {
        public int Id { get; set; }
        public AttendanceStatus AttendanceStatus { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public string StartImageFileName { get; set; } = string.Empty;
        public DateTimeOffset? EndTimestamp { get; set; }
        public string? EndImageFileName { get; set; }


        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = new Employee();
    }

    public class AttendanceRequest
    {
        public AttendanceStatus AttendanceStatus { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public string StartImageFileName { get; set; } = string.Empty;
        public DateTimeOffset? EndTimestamp { get; set; }
        public string? EndImageFileName { get; set; }

        public string EmployeeNationalId { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
    }
	
	public class AttendanceResponse
    {
        public AttendanceStatus AttendanceStatus { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public string StartImageFileName { get; set; } = string.Empty;
        public DateTimeOffset? EndTimestamp { get; set; }
        public string? EndImageFileName { get; set; }

        public string EmployeeNationalId { get; set; } = string.Empty;
        public string EmployeeFullName { get; set; } = string.Empty;
    }
}
