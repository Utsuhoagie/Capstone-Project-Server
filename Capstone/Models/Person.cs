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
		public string? Email { get; set; }
		public int ExperienceYears { get; set; }
	}

	public class PersonDto
	{
		public string NationalId { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Gender { get; set; } = string.Empty;
		public DateTimeOffset? BirthDate { get; set; }
		public string Address { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int ExperienceYears { get; set; }
	}
}