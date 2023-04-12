using System.ComponentModel.DataAnnotations;

namespace Capstone.Models
{
	public class Applicant: Person
	{
		public DateTimeOffset AppliedDate { get; set; }
		public int AskingSalary { get; set; }

		public int AppliedPositionId { get; set; }
		public Position AppliedPosition { get; set; } = new Position();
	}

	public class ApplicantDto: PersonDto
	{
		public DateTimeOffset AppliedDate { get; set; }
		public int AskingSalary { get; set; }

		public string AppliedPositionName { get; set; } = string.Empty;
	}
}
