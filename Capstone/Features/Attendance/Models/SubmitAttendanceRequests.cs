namespace Capstone.Features.AttendanceModule.Models
{
    public class StartAttendanceRequest
    {
        public string EmployeeNationalId { get; set; } = string.Empty;
        public string QrHash { get; set; } = string.Empty;
        public DateTimeOffset StartTimestamp { get; set; }
        public IFormFile StartImage { get; set; } = default!;
    }

    public class EndAttendanceRequest
    {
        public string EmployeeNationalId { get; set; } = string.Empty;
        public string QrHash { get; set; } = string.Empty;
        public DateTimeOffset EndTimestamp { get; set; }
        public IFormFile EndImage { get; set; } = default!;
    }
}
