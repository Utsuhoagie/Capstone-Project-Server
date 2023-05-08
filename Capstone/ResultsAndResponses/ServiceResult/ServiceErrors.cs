namespace Capstone.ResultsAndResponses.ServiceResult
{
	public class ServiceErrors
	{
		// Auth Errors
		public static readonly string DuplicateEmployeeUserError = "Tài khoản nhân viên đã tồn tại.";
		public static readonly string NoEmployeeUserError = "Tài khoản nhân viên không tồn tại.";
		public static readonly string PasswordsDontMatchError = "Mật khẩu không trùng khớp.";

		// App Errors
		public static readonly string StartAfterEndDateError = "Thời gian bắt đầu phải trước thời gian kết thúc.";

		public static readonly string DuplicatePersonError = "Số CMND/CCCD đã tồn tại.";
		public static readonly string NoApplicantError = "Ứng viên không tồn tại.";
		public static readonly string NoEmployeeError = "Nhân viên không tồn tại.";
		public static readonly string EmployeeNotSelfError = "Nhân viên không được phép cập nhật của nhân viên khác.";

		public static readonly string NoAttendanceError = "Đợt chấm công không tồn tại.";
		public static readonly string AttendanceNotEndedError = "Đợt chấm công chưa kết thúc.";
		public static readonly string AttendanceAlreadyStartedError = "Đợt chấm công đã bắt đầu.";
		public static readonly string AttendanceAlreadyEndedError = "Đợt chấm công đã kết thúc.";
		public static readonly string AttendanceAlreadyUpdatedError = "Đợt chấm công đã được kiểm tra.";
		public static readonly string AttendanceAlreadyRejectedError = "Đợt chấm công đã bị từ chối.";
		
		public static readonly string NoRequestError = "Yêu cầu không tồn tại.";
		public static readonly string NoNewSalaryError = "Yêu cầu không có mức lương mới.";
		public static readonly string NoLeaveDateError = "Yêu cầu không có đầy đủ ngày bắt đầu/kết thúc nghỉ phép.";

		public static readonly string EmployeeOnLeaveError = "Nhân viên đang nghỉ phép.";

		public static readonly string DuplicatePositionError = "Vị trí đã tồn tại.";
		public static readonly string NoPositionError = "Vị trí không tồn tại.";
		public static readonly string PositionHasPeopleError = "Vị trí vẫn còn được sử dụng.";
	}
}
