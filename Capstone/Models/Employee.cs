namespace Capstone.Models
{
	public class Employee: Person
	{
		public int PositionId { get; set; }
		public Position Position { get; set; } = new Position();

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public int StartHour { get; set; }

		public int EndHour { get; set; }

		public EmployeeUser? User { get; set; }
	}

	public class EmployeeDto : PersonDto
	{
		public string PositionName { get; set; } = string.Empty;

		public int Salary { get; set; }

		public DateTimeOffset EmployedDate { get; set; }

		public int StartHour { get; set; }

		public int EndHour { get; set; }

		public bool HasUser { get; set; }
	}
}
