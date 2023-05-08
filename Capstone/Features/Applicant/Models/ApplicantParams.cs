namespace Capstone.Features.ApplicantModule.Models
{
    public class ApplicantParams
    {
		// Non-range
		public string? NationalId { get; set; }
        public string? NamePart { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AppliedPositionName { get; set; }

		// Range
        public DateTimeOffset? AppliedDateFrom { get; set; }
        public DateTimeOffset? AppliedDateTo { get; set; }

        public int? AskingSalaryFrom { get; set; }
        public int? AskingSalaryTo { get; set; }

		public int? ExperienceYearsFrom { get; set; }
		public int? ExperienceYearsTo { get; set; }
	}
}
