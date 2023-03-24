namespace Capstone.Models
{
	public class Position
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;

		public List<Applicant> Applicants { get; set; } = new List<Applicant>();
		public List<Employee> Employees { get; set; } = new List<Employee>();
	}

	public class PositionDto
	{
		public string Name { get; set; } = string.Empty;

		public int ApplicantCount { get; set; }
		public int EmployeeCount { get; set; }
	}
}
