namespace Capstone.ResultsAndResponses.ServiceResult
{
	public class ServiceErrors
	{
		// Auth Errors
		public static readonly string DuplicateEmployeeUserError = "Tài khoản nhân viên đã tồn tại.";
		public static readonly string NoEmployeeUserError = "Tài khoản nhân viên không tồn tại.";
		public static readonly string PasswordsDontMatchError = "Mật khẩu không trùng khớp.";

		// App Errors
		public static readonly string DuplicatePersonError = "Số CMND/CCCD đã tồn tại.";
		public static readonly string NoApplicantError = "Ứng viên không tồn tại.";
		public static readonly string NoEmployeeError = "Nhân viên không tồn tại.";

		public static readonly string DuplicatePositionError = "Vị trí đã tồn tại.";
		public static readonly string NoPositionError = "Vị trí không tồn tại.";
		public static readonly string PositionHasPeopleError = "Vị trí vẫn còn được sử dụng.";
	}
}
