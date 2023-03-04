namespace Capstone.Features.EmployeeModule
{
	public class EmployeeFilterParams
	{
		public string? SubName { get; set; }
		public string? Gender { get; set; }
		public string? Address { get; set; }
		public int? ExperienceYears { get; set; }
		public string? Position { get; set; }
		public int? Salary { get; set; }
		public DateTimeOffset? EmployedDateFrom { get; set; }
		public DateTimeOffset? EmployedDateTo { get; set; }
	}
}
