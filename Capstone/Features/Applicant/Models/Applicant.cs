using Capstone.Features.PositionModule.Models;
using Capstone.Models;

namespace Capstone.Features.ApplicantModule.Models
{
    public class Applicant : Person
    {
        public DateTimeOffset AppliedDate { get; set; }
        public int AskingSalary { get; set; }

        public int AppliedPositionId { get; set; }
        public Position AppliedPosition { get; set; } = new Position();
    }

    public class ApplicantRequest : PersonRequest
    {
        public DateTimeOffset AppliedDate { get; set; }
        public int AskingSalary { get; set; }

        public string AppliedPositionName { get; set; } = string.Empty;
    }

	public class ApplicantResponse : PersonResponse
	{
		public DateTimeOffset AppliedDate { get; set; }
		public int AskingSalary { get; set; }

		public string AppliedPositionName { get; set; } = string.Empty;
	}
}
