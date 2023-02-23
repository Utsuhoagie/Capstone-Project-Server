namespace Capstone.Models
{
	public class Employee: Person
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public DateTimeOffset StartTime { get; set; }

		public DateTimeOffset EndTime { get; set; }
	}

	public class EmployeeDto : PersonDto
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public DateTimeOffset StartTime { get; set; }

		public DateTimeOffset EndTime { get; set; }
	}
}
