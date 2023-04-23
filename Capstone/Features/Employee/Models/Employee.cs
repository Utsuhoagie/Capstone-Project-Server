using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.Auth.Models;
using Capstone.Features.FeedbackModule.Models;
using Capstone.Features.LeaveModule.Models;
using Capstone.Features.PositionModule.Models;
using Capstone.Models;

namespace Capstone.Features.EmployeeModule.Models
{
    public class Employee : Person
    {
        public int Salary { get; set; }
        public DateTimeOffset EmployedDate { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = new Position();
		public List<Leave> Leaves { get; set; } = new List<Leave>();
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
		public List<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public EmployeeUser? User { get; set; }
    }

    public class EmployeeRequest : PersonRequest
    {
        public int Salary { get; set; }
        public DateTimeOffset EmployedDate { get; set; }

        public string PositionName { get; set; } = string.Empty;
        //public bool HasUser { get; set; }
    }

	public class EmployeeResponse : PersonResponse
	{
		public int Salary { get; set; }
		public DateTimeOffset EmployedDate { get; set; }

		public string PositionName { get; set; } = string.Empty;
		public bool HasUser { get; set; }
	}
}
