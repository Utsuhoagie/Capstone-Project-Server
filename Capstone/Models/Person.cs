namespace Capstone.Models
{
	public enum GenderEnum { Male, Female, Other }

	public class Person
	{
		public int Id { get; set; }

		public string NationalId { get; set; } = string.Empty;

		public string FullName { get; set; } = string.Empty;

		public GenderEnum Gender { get; set; }

		public DateTime BirthDate { get; set; }

		public string Address { get; set; } = string.Empty; 

		public string Phone { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public int ExperienceYears { get; set; }
	}
}
