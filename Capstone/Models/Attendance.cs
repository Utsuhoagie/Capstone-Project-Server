namespace Capstone.Models
{
	public enum Status
	{
		Pending = 0, 
		Accepted = 1, 
		Rejected = -1 
	}

	public class Attendance
	{
		public int Id { get; set; }
		public Status Status { get; set; }
		public DateTimeOffset StartTimestamp { get; set; }
		public string StartImageFileName { get; set; } = string.Empty;
		public DateTimeOffset? EndTimestamp { get; set; }
		public string? EndImageFileName { get; set; }
		
		
		public int EmployeeId { get; set; }
		public Employee Employee { get; set; } = new Employee();
	}
	
	public class AttendanceDto
	{
		public Status Status { get; set; }
		public DateTimeOffset StartTimestamp { get; set; }
		public string StartImageFileName { get; set; } = string.Empty;
		public DateTimeOffset? EndTimestamp { get; set; }
		public string? EndImageFileName { get; set; }

		public string EmployeeNationalId { get; set; } = string.Empty;
		public string EmployeeFullName { get; set; } = string.Empty;
	}
}
