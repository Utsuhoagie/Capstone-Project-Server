namespace Capstone.Models
{
	public class Applicant: Person
	{
		public string AppliedPosition { get; set; } = string.Empty;

		public DateTime AppliedDate { get; set; }

		public int AskingSalary { get; set; }
	}
}
