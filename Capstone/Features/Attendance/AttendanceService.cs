using Capstone.Data;
using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Features.FileModule;
using Capstone.Responses.Pagination;
using Capstone.Responses.ServiceResponse;
using Capstone.ResultsAndResponses.ServiceResult;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Capstone.Features.AttendanceModule
{
	public interface IAttendanceService
	{
		// ==== Web ====
		string GetDailyHash();
		Task<List<DailyStatus>> GetDailyAttendanceStatusesOfMonth(DateTimeOffset vnDate);
		Task<PagedResult<EmployeeResponse>> GetEmployeesNotOnLeave(PagingParams pagingParams, DateTimeOffset vnDate);
		Task<AttendanceResponse?> GetAttendanceOfEmployee(string NationalId, DateTimeOffset vnDate);
		Task<ServiceResult> BatchUpdatePreviousDaysOfMonth(string type, DateTimeOffset vnDate);
		Task<ServiceResult> UpdateStatus(UpdateStatusRequest req);

		// ==== Mobile ====
		// returns "Empty", "Started" or "Ended"
		Task<string?> CheckAttendanceToday(string NationalId, DateTimeOffset vnDate);
		Task<ServiceResult> StartAttendance(StartAttendanceRequest req);
		Task<ServiceResult> EndAttendance(EndAttendanceRequest req);

		//Task<PagedResult<ApplicantRequest>> GetApplicants(PagingParams pagingParams, ApplicantFilterParams filterParams);

		//Task<ApplicantRequest?> GetApplicant(string NationalId);

		//Task<ServiceResult> DeleteApplicant(string NationalId);

		//Task<ServiceResult> EmployApplicant(string NationalId, EmployeeRequest employeeDto);

		Task<ServiceResult> DEBUG_DELETE();
	}

	public class AttendanceService: IAttendanceService
	{
		private readonly CapstoneContext _context;
		private readonly IConfiguration _configuration;
		private readonly string DANGEROUS_FILE_PATH;

		public AttendanceService(CapstoneContext context, IConfiguration configuration) 
		{
			_context = context;
			_configuration = configuration;
			DANGEROUS_FILE_PATH = $"{_configuration.GetSection("FilePath").Value}\\Attendances";
		}

		#region==== Web ====
		public string GetDailyHash()
		{
			var secretString = _configuration.GetSection("JWT:SecretKey").Value;
			var secretBytes = Encoding.UTF8.GetBytes(secretString);

			var hmacSHA256 = new HMACSHA256(secretBytes);

			var dailyHashRawString = $"{DateTimeOffset.UtcNow.Date} {secretString}";
			var dailyHashRawBytes = Encoding.UTF8.GetBytes(dailyHashRawString);
			var dailyHashBytes = hmacSHA256.ComputeHash(dailyHashRawBytes);

			var sb = new StringBuilder();

			foreach (var dailyHashByte in dailyHashBytes) 
			{
				sb.Append(dailyHashByte.ToString("X2"));
			}

			return sb.ToString();
		}

		
		public async Task<List<DailyStatus>> GetDailyAttendanceStatusesOfMonth(DateTimeOffset vnDate)
		{
			var currentDate = DateTimeOffset.Now;

			var day = vnDate.Day;
			var month = vnDate.Month;
			var year = vnDate.Year;

			var attendancesInMonth = await _context.Attendances
				.Where(a => a.StartTimestamp.Month == month && a.StartTimestamp.Year == year)
				.ToListAsync();

			var daysInMonth = DateTime.DaysInMonth(year, month);

			var dailyStatusList = new List<DailyStatus>(
				Enumerable.Repeat(DailyStatus.Empty, daysInMonth));
			
			for (int _day = 1; _day <= daysInMonth; _day++)
			{
				var dateInMonth = new DateTimeOffset(
						year, month, _day,
						0, 0, 0,
						new TimeSpan(7, 0, 0));

				// Future dates are just DailyStatus.Empty
				if (dateInMonth.Date > currentDate)
				{
					continue;
				}

				var dayOfWeek = dateInMonth.DayOfWeek;

				if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
				{
					continue;
				}

				var attendancesInDate = attendancesInMonth
					.Where(a => a.StartTimestamp.Day == _day);
				
				if (attendancesInDate.Count() == 0)
				{
					var employeeResponsesNotOnLeaveOnDay = (await
						GetEmployeesNotOnLeave(
							new PagingParams { Page = 1, PageSize = 30 },
							dateInMonth
							)
						)
						.Items;

					// No Attendances today, BUT
					// all Employees are on Leave too
					// -> DailyStatus is Empty (white on FE)
					if (employeeResponsesNotOnLeaveOnDay.Count() == 0)
					{
						dailyStatusList[_day - 1] = DailyStatus.Empty;
					}

					// No Attendances today, AND
					// some Employees are NOT on Leave
					// they must submit Attendances
					// -> DailyStatus is Pending (red on FE)

					// If they still don't, if this vnDate is BEFORE today
					// admin can reject this + mark as Violation
					else
					{
						dailyStatusList[_day - 1] = DailyStatus.Empty;
						//dailyStatusList[_day - 1] = DailyStatus.Pending;
					}

					continue;
				}


				// Don't need to check Employees Not On Leave here
				// because by default,
				// only Employees Not On Leave
				// can submit Attendances
				var dailyStatus = attendancesInDate
					.Any(a => a.AttendanceStatus == AttendanceStatus.Pending) ?
						DailyStatus.Pending :
						DailyStatus.Finished;

				dailyStatusList[_day - 1] = dailyStatus;
			}

			return dailyStatusList;
		}

		public async Task<PagedResult<EmployeeResponse>> GetEmployeesNotOnLeave(
			PagingParams pagingParams, DateTimeOffset vnDate)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			//var debug = _context.People.OfType<Employee>()
			//	.Include(e => e.Attendances)
			//	.Include(e => e.Leaves)
			//	.Include(e => e.Position)
			//	.Include(e => e.User);
			//foreach (var emp in debug)
			//{
			//	foreach (var leave in emp.Leaves)
			//	{
			//		var startDate = leave.StartDate.Date;
			//		var endDate = leave.EndDate.Date;
			//	}
			//}
				// Only include Attendances where the Employee has no Leave
				// means: at least 1 Leave of Employee is
				// ---[Start...vnDate...End]---
				//.Where(e => e.Leaves
				//	.Any(l =>
				//		(l.StartDate.Date <= vnDate) &&
				//		(l.EndDate.Date >= vnDate)
				//	)
				//	|| e.Leaves.Count == 0);

			var clievalEmployeeResponses = (await _context.People.OfType<Employee>()
				.Include(e => e.Attendances)
				.Include(e => e.Leaves)
				.Include(e => e.Position)
				.Include(e => e.User)

				// Only include Attendances where the Employee has no Leave
				// or Employee has Leaves that are
				// ---vnDate---[Start...End]---
				// ---[Start...End]---vnDate---
				//
				// means: at least 1 Leave of Employee is
				.ToListAsync())
				.Where(e => e.EmployedDate.Date <= vnDate)
				.Where(e => e.Leaves.Count == 0 || e.Leaves
					.Any(l =>
						(l.StartDate.Date > vnDate) ||
						(l.EndDate.Date < vnDate)
					))
				/*.Where(a => (
					 a.StartTimestamp is +0:00 time, correct time but with diff offset
					 vnDate is +0:00 time, correct time but with diff offset
					 have to compare and check for "Same Day"
					 a.StartTimestamp is between[17:00 day1, 16:59 day2]
					a.StartTimestamp.AddHours(7).Date == vnDate.Date
					)
				(a.StartTimestamp.Day == vnDate.Day) &&
				(a.StartTimestamp.Month == vnDate.Month) &&
				(a.StartTimestamp.Year == vnDate.Year)
				)*/
				.Select(e => new EmployeeResponse
				{
					NationalId = e.NationalId,
					FullName = e.FullName,
					Gender = e.Gender,
					Address = e.Address,
					BirthDate = e.BirthDate,
					Email = e.Email,
					Phone = e.Phone,
					EmployedDate = e.EmployedDate,
					PositionName = e.Position.Name,
					ExperienceYears = e.ExperienceYears,

					Salary = e.Salary,
					HasUser = e.User != null,
					ImageFileName = e.ImageFileName,

					/*EmployeeFullName = a.Employee.FullName,
					EmployeeNationalId = a.Employee.NationalId,
					AttendanceStatus = a.AttendanceStatus,
					StartTimestamp = a.StartTimestamp,
					StartImageFileName = a.StartImageFileName,
					EndTimestamp = a.EndTimestamp,
					EndImageFileName = a.EndImageFileName,*/
				});

			var totalCount = clievalEmployeeResponses.Count();

			var pagedEmployeeResponses = clievalEmployeeResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PagedResult<EmployeeResponse>(
				items: pagedEmployeeResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
		}

		public async Task<AttendanceResponse?> GetAttendanceOfEmployee(string NationalId, DateTimeOffset vnDate)
		{
			//var employee = await _context.People.OfType<Employee>()
			//	.SingleOrDefaultAsync(e => e.NationalId == NationalId);

			//if (employee == null)
			//{
			//	return new 

			//}

			var attendance = await _context.Attendances
				.Include(a => a.Employee)
				.SingleOrDefaultAsync(a =>
					(a.Employee.NationalId == NationalId) &&
					(a.StartTimestamp.Date == vnDate.Date)
				);

			if (attendance == null)
			{
				return null;
			}

			var attendanceRes = new AttendanceResponse
			{
				AttendanceStatus = attendance.AttendanceStatus,
				StartTimestamp = attendance.StartTimestamp,
				StartImageFileName = attendance.StartImageFileName,
				EndTimestamp = attendance.EndTimestamp,
				EndImageFileName = attendance.EndImageFileName,
				EmployeeFullName = attendance.Employee.FullName,
				EmployeeNationalId = attendance.Employee.NationalId,
			};
			
			return attendanceRes;
		}

		public async Task<ServiceResult> BatchUpdatePreviousDaysOfMonth(string type, DateTimeOffset vnDate)
		{
			// type = 'Accept' || 'Reject'

			var previousAttendancesOfMonth = await _context.Attendances
				.Where(a => a.StartTimestamp.Date < vnDate.Date)
				.ToListAsync();

			foreach (var attendance in previousAttendancesOfMonth)
			{
				var newAttendanceStatus =
					type == "Accept" ? AttendanceStatus.Accepted :
					type == "Reject" ? AttendanceStatus.Rejected :
					attendance.AttendanceStatus;
				attendance.AttendanceStatus = newAttendanceStatus;
			}

			return new ServiceResult
			{
				Success = true,
			};
		}

		public async Task<ServiceResult> UpdateStatus(UpdateStatusRequest req)
		{
			var attendance = await _context.Attendances
				.Include(a => a.Employee)
				.FirstOrDefaultAsync(a => a.Employee.NationalId == req.EmployeeNationalId
					&& a.StartTimestamp.Date == req.StartTimestamp.Date);

			if (attendance == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoAttendanceError,
				};
			}

			if (attendance.EndTimestamp == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.AttendanceNotEndedError,
				};
			}

			attendance.AttendanceStatus = req.Status;
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}
		#endregion


		#region==== Mobile ====
		public async Task<string?> CheckAttendanceToday(string NationalId, DateTimeOffset vnDate)
		{
			var employee = await _context.People.OfType<Employee>()
				.Include(e => e.Attendances)
				.FirstOrDefaultAsync(e => e.NationalId == NationalId);

			if (employee == null)
			{
				return null;
			}

			var attendanceToday = employee.Attendances
				.Where(a => a.StartTimestamp.Date == vnDate.Date)
				.SingleOrDefault();

			if (attendanceToday == null)
			{
				return "Empty";
			}

			if (attendanceToday.EndTimestamp == null)
			{
				return "Started";
			}
			else
			{
				return "Ended";
			}
		}

		public async Task<ServiceResult> StartAttendance(StartAttendanceRequest req)
		{
			// Find employee
			var employee = await _context.People.OfType<Employee>()
				.FirstOrDefaultAsync(e => e.NationalId == req.EmployeeNationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
				};
			}

			// Check if vnDate is on leave for employee
			var isDateInAnyLeave = await _context.Leaves
				.Include(l => l.Employee)
				.Where(l => l.Employee.NationalId == req.EmployeeNationalId)
				.AnyAsync(l =>
					(l.StartDate.Date <= req.StartTimestamp.Date) &&
					(l.EndDate.Date >= req.StartTimestamp.Date));

			if (isDateInAnyLeave)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.EmployeeOnLeaveError
				};
			}

			// Check hash
			var dailyHash = GetDailyHash();
			var isHashCorrect = req.QrHash == dailyHash;

			// Check existing AttendanceModule
			var existingAttendance = await _context.Attendances
				.Include(a => a.Employee)
				.FirstOrDefaultAsync(a => a.Employee.NationalId == req.EmployeeNationalId 
					&& a.StartTimestamp.Date == req.StartTimestamp.Date);

			if (existingAttendance != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.AttendanceAlreadyStartedError
				};
			}

			// Upload file
			var startImage = req.StartImage;
			var startT = req.StartTimestamp;
			var safeFileNameTimestamp =
				$"{startT.Day}-{startT.Month}-{startT.Year}_{startT.Hour}-{startT.Minute}";
			var safeFileName = $"{safeFileNameTimestamp}_{employee.NationalId}_START";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, safeFileName);
			var safeFilePathNameWithCorrectExtension =
				Path.ChangeExtension(safeFilePathName, "jpeg");

			using (var fileStream = System.IO.File.Create(safeFilePathNameWithCorrectExtension))
			{
				await startImage.CopyToAsync(fileStream);
			}

			// Add attendance, with NULL End fields
			var attendance = new Attendance
			{
				StartTimestamp = req.StartTimestamp,
				StartImageFileName = Path.GetFileName(safeFilePathNameWithCorrectExtension),
				AttendanceStatus = isHashCorrect ? AttendanceStatus.Pending : AttendanceStatus.Rejected,
				Employee = employee,
			};
			await _context.Attendances.AddAsync(attendance);

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true
			};
		}

		public async Task<ServiceResult> EndAttendance(EndAttendanceRequest req)
		{
			// Find employee
			var employee = await _context.People.OfType<Employee>()
				.FirstOrDefaultAsync(e => e.NationalId == req.EmployeeNationalId);

			if (employee == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoEmployeeError
				};
			}

			// Check if vnDate is on leave for employee
			var isDateInAnyLeave = await _context.Leaves
				.Include(l => l.Employee)
				.Where(l => l.Employee.NationalId == req.EmployeeNationalId)
				.AnyAsync(l =>
					(l.StartDate.Date <= req.EndTimestamp.Date) &&
					(l.EndDate.Date >= req.EndTimestamp.Date));

			if (isDateInAnyLeave)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.EmployeeOnLeaveError
				};
			}

			// Check hash
			var dailyHash = GetDailyHash();
			var isHashCorrect = req.QrHash == dailyHash;

			// Find existing AttendanceModule,
			// which should only has Start
			// should have same Date (no time)
			var attendance = await _context.Attendances
				.Include(a => a.Employee)
				.FirstOrDefaultAsync(a => a.Employee.NationalId == req.EmployeeNationalId
					&& a.StartTimestamp.Date == req.EndTimestamp.Date);

			if (attendance == null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.NoAttendanceError
				};
			}
			if (attendance.EndTimestamp != null)
			{
				return new ServiceResult
				{
					Success = false,
					ErrorMessage = ServiceErrors.AttendanceAlreadyEndedError
				};
			}

			// Upload file
			var endImage = req.EndImage;
			var endT = req.EndTimestamp;
			//var safeFileName = Path.GetRandomFileName();
			var safeFileNameTimestamp =
				$"{endT.Day}-{endT.Month}-{endT.Year}_{endT.Hour}-{endT.Minute}";
			var safeFileName = $"{safeFileNameTimestamp}_{employee.NationalId}_END";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, safeFileName);
			var safeFilePathNameWithCorrectExtension =
				Path.ChangeExtension(safeFilePathName, "jpeg");

			using (var fileStream = System.IO.File.Create(safeFilePathNameWithCorrectExtension))
			{
				await endImage.CopyToAsync(fileStream);
			}

			// Update attendance to include End fields
			attendance.EndTimestamp = DateTimeOffset.UtcNow;
			attendance.EndImageFileName = Path.GetFileName(safeFilePathNameWithCorrectExtension);

			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true
			};
		}
		#endregion

		#region==== DEBUG ====
		public async Task<ServiceResult> DEBUG_DELETE()
		{
			var di = new DirectoryInfo(DANGEROUS_FILE_PATH);

			foreach (var file in di.EnumerateFiles())
			{
				file.Delete();
			}

			_context.Attendances.RemoveRange(_context.Attendances.ToList());
			_context.SaveChanges();

			return new ServiceResult
			{
				Success = true
			};
		}
		#endregion
	}
}
