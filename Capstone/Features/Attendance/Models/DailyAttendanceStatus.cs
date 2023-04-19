using Capstone.Models;

namespace Capstone.Features.AttendanceModule.Models
{
	public enum DailyStatus { Pending = 0, Finished = 1, Empty = -1 }
	public class DailyAttendanceStatus
	{
		public int Day { get; set; }
		public DailyStatus DailyStatus { get; set; }
	}
}
