using Capstone.Features.EmployeeModule.Models;

namespace Capstone.Features.FeedbackModule.Models
{
	public class Feedback
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTimeOffset CreatedDate { get; set; }

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; } = default!;
	}

	public class AddFeedbackRequest
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public string EmployeeNationalId { get; set; } = string.Empty;
	}

	public class FeedbackResponse
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTimeOffset CreatedDate { get; set; }

		public string EmployeeFullName { get; set; } = string.Empty;
	}
}
