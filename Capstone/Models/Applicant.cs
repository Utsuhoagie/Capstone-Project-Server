using System.ComponentModel.DataAnnotations;

namespace Capstone.Models
{
	public class Applicant: Person
	{
		public string AppliedPosition { get; set; } = string.Empty;

		public DateTimeOffset AppliedDate { get; set; }

		public int AskingSalary { get; set; }
	}

	public class ApplicantDto: PersonDto
	{
		public string AppliedPosition { get; set; } = string.Empty;

		public DateTimeOffset AppliedDate { get; set; }

		public int AskingSalary { get; set; }
	}
}
