namespace Capstone.Models
{
	public class Employee: Person
	{
		public int Salary { get; set; }
		public DateTimeOffset EmployedDate { get; set; }
		public int StartHour { get; set; }
		public int EndHour { get; set; }

		public int PositionId { get; set; }
		public Position Position { get; set; } = new Position();
		public List<Attendance> Attendances { get; set; } = new List<Attendance>();
		public EmployeeUser? User { get; set; }
	}

	public class EmployeeDto : PersonDto
	{
		public int Salary { get; set; }
		public DateTimeOffset EmployedDate { get; set; }
		public int StartHour { get; set; }
		public int EndHour { get; set; }

		public string PositionName { get; set; } = string.Empty;
		public bool HasUser { get; set; }
	}
}
