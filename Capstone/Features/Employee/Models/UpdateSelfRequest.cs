namespace Capstone.Features.EmployeeModule.Models
{
	public class UpdateSelfRequest
	{
		public string NationalId { get; set; } = string.Empty;
		public string? Phone { get; set; }
		public string? Address { get; set; }
		public DateTimeOffset? BirthDate { get; set; }
		public IFormFile? Image { get; set; }
		public IFormFile? Resume { get; set; }
	}
}
