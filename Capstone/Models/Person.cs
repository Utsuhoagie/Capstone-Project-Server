namespace Capstone.Models
{
	public class Person
	{
		public int Id { get; set; }
		public string NationalId { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Gender { get; set; } = string.Empty;
		public DateTimeOffset? BirthDate { get; set; }
		public string Address { get; set; } = string.Empty; 
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int ExperienceYears { get; set; }
		public string? ImageFileName { get; set; }
		public string? ResumeFileName { get; set; }
	}

	public class PersonRequest
	{
		public string NationalId { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Gender { get; set; } = string.Empty;
		public DateTimeOffset? BirthDate { get; set; }
		public string Address { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int ExperienceYears { get; set; }
		//public string? ImageFileName { get; set; }
		public IFormFile? Image { get; set; }
		public IFormFile? Resume { get; set; }
	}

	public class PersonResponse
	{
		public string NationalId { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Gender { get; set; } = string.Empty;
		public DateTimeOffset? BirthDate { get; set; }
		public string Address { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public int ExperienceYears { get; set; }
		public string? ImageFileName { get; set; }
		public string? ResumeFileName { get; set; }
	}
}