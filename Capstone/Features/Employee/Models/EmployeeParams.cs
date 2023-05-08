namespace Capstone.Features.EmployeeModule.Models
{
    public class EmployeeParams
    {
		// Non-range
		public string? NationalId { get; set; }
		public string? NamePart { get; set; }
		public string? Gender { get; set; }
		public string? Address { get; set; }
		public string? PositionName { get; set; }

		// Range
		public DateTimeOffset? EmployedDateFrom { get; set; }
		public DateTimeOffset? EmployedDateTo { get; set; }

		public int? SalaryFrom { get; set; }
		public int? SalaryTo { get; set; }

		public int? ExperienceYearsFrom { get; set; }
		public int? ExperienceYearsTo { get; set; }
    }
}
