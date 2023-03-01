namespace Capstone.Features.ApplicantTracking
{
	public class ApplicantTrackingFilterParams
	{
		public string? SubName { get; set; }
		public string? Gender { get; set; }
		public string? Address { get; set; }
		public int? ExperienceYears { get; set; }
		public string? AppliedPosition { get; set; }
		public DateTimeOffset? AppliedDateFrom { get; set; }
		public DateTimeOffset? AppliedDateTo { get; set; }
		public int? AskingSalary { get; set; }
	}
}
