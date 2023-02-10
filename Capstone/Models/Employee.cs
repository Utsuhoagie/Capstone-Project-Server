namespace Capstone.Models
{
	public class Employee: Person
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTime EmployedDate { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }
	}

	public class EmployeeDto : PersonDto
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTime EmployedDate { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }
	}
}
