using Capstone.Data;
using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.AttendanceModule.Models;
using Capstone.Features.EmployeeModule.Models;
using Capstone.Features.File;
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

namespace Capstone.Features.AttendanceModule
{
	public interface IAttendanceService
	{
		string GetDailyHash();
		Task<PagedResult<AttendanceResponse>> GetDailyAttendances(PagingParams pagingParams, DateOnly dateOnly);
		Task<List<DailyStatus>> GetDailyAttendanceStatusesOfMonth(DateOnly dateOnly);
		Task<ServiceResult> StartAttendance(StartAttendanceRequest req);
		Task<ServiceResult> EndAttendance(EndAttendanceRequest req);
		Task<ServiceResult> UpdateStatus(UpdateStatusRequest req);

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

		
		public async Task<List<DailyStatus>> GetDailyAttendanceStatusesOfMonth(DateOnly dateOnly)
		{
			var day = dateOnly.Day;
			var month = dateOnly.Month;
			var year = dateOnly.Year;

			var attendancesInMonth = await _context.Attendances
				.Where(a => a.StartTimestamp.Month == month && a.StartTimestamp.Year == year)
				.ToListAsync();

			var daysInMonth = DateTime.DaysInMonth(year, month);

			var dailyStatusList = new List<DailyStatus>(
				Enumerable.Repeat(DailyStatus.Empty, daysInMonth));
			
			for (int _day = 1; _day <= daysInMonth; _day++)
			{
				var attendancesInDate = attendancesInMonth
					.Where(a => a.StartTimestamp.Day == _day);

				if (attendancesInDate.Count() == 0)
				{
					continue;
				}

				var dailyStatus = attendancesInDate
					.Any(a => a.Status == Status.Pending) ?
						DailyStatus.Pending :
						DailyStatus.Finished;

				dailyStatusList[_day - 1] = dailyStatus;
			}

			return dailyStatusList;
		}

		public async Task<PagedResult<AttendanceResponse>> GetDailyAttendances(
			PagingParams pagingParams, 
			DateOnly dateOnly)
		{
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var queryableDailyAttendanceResponses = _context.Attendances
				.Include(a => a.Employee)
				.Where(a => 
					a.StartTimestamp.Day == dateOnly.Day && 
					a.StartTimestamp.Month == dateOnly.Month &&
					a.StartTimestamp.Year == dateOnly.Year
				)
				.Select(a => new AttendanceResponse
				{
					EmployeeFullName = a.Employee.FullName,
					EmployeeNationalId = a.Employee.NationalId,
					Status = a.Status,
					StartTimestamp = a.StartTimestamp,
					StartImageFileName = a.StartImageFileName,
					EndTimestamp = a.EndTimestamp,
					EndImageFileName = a.EndImageFileName,
				});

			var totalCount = await queryableDailyAttendanceResponses.CountAsync();

			var pagedAttendanceResponses = queryableDailyAttendanceResponses
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return new PagedResult<AttendanceResponse>(
				items: pagedAttendanceResponses,
				totalCount: totalCount,
				page: page,
				pageSize: pageSize);
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
			var safeFileName = $"{safeFileNameTimestamp}_{employee.NationalId}";
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
				StartTimestamp = DateTimeOffset.UtcNow,
				StartImageFileName = Path.GetFileName(safeFilePathNameWithCorrectExtension),
				Status = isHashCorrect ? Status.Pending : Status.Rejected,
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
			var safeFileName = $"{safeFileNameTimestamp}_{employee.NationalId}";
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

			attendance.Status = req.Status;
			await _context.SaveChangesAsync();

			return new ServiceResult
			{
				Success = true,
			};
		}

		#region------DEBUG------
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
