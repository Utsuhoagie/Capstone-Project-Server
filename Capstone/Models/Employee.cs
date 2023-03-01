namespace Capstone.Models
{
	public class Employee: Person
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public int StartHour { get; set; }

		public int EndHour { get; set; }
	}

	public class EmployeeDto : PersonDto
	{
		public string Position { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public int StartHour { get; set; }

		public int EndHour { get; set; }
	}
}
