using System.ComponentModel.DataAnnotations;

namespace Capstone.Models
{
	public class Applicant: Person
	{
		public int AppliedPositionId { get; set; }
		public Position AppliedPosition { get; set; } = new Position();

		public DateTimeOffset AppliedDate { get; set; }

		public int AskingSalary { get; set; }
	}

	public class ApplicantDto: PersonDto
	{
		public string AppliedPositionName { get; set; } = string.Empty;

		public DateTimeOffset AppliedDate { get; set; }

		public int AskingSalary { get; set; }
	}
}
