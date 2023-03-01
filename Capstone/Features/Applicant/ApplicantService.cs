using Capstone.Data;
using Capstone.ExceptionHandling;
using Capstone.Models;
using Capstone.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Capstone.Features.ApplicantModule
{
    public class ApplicantService : IApplicantService
    {
        private readonly CapstoneContext _context;
		private readonly IValidator<ApplicantDto> _validator;

		public ApplicantService(
			CapstoneContext capstoneContext, 
			IValidator<ApplicantDto> validator)
        {
            _context = capstoneContext;
			_validator = validator;
        }

		public async Task<PagedResult<ApplicantDto>> GetAllApplicantsAsync()
		{
			var applicants = await _context.Applicant
				.Select(a => new ApplicantDto
					{
						NationalId = a.NationalId,
						FullName = a.FullName,
						Gender = a.Gender,
						BirthDate = a.BirthDate,
						Address = a.Address,
						Phone = a.Phone,
						Email = a.Email,
						ExperienceYears = a.ExperienceYears,
						AppliedPosition = a.AppliedPosition,
						AppliedDate = a.AppliedDate,
						AskingSalary = a.AskingSalary,
					})
				.ToListAsync();

			var totalCount = await _context.Applicant.CountAsync();
			
			return new PagedResult<ApplicantDto>(applicants, totalCount, 1);
		}

        public async Task<PagedResult<ApplicantDto>> GetApplicantsAsync(
			PagingParams pagingParams,
			ApplicantFilterParams filterParams)
        {
			var page = pagingParams.Page;
			var pageSize = pagingParams.PageSize;

			var SubName = filterParams.SubName;
			var Gender = filterParams.Gender;
			var Address = filterParams.Address;
			var ExperienceYears = filterParams.ExperienceYears;
			var AppliedPosition = filterParams.AppliedPosition;
			var AppliedDateFrom = filterParams.AppliedDateFrom;
			var AppliedDateTo = filterParams.AppliedDateTo;
			var AskingSalary = filterParams.AskingSalary;

			var queryableFilteredApplicantDtos = _context.Applicant
				.Where(a => SubName == null || a.FullName.ToLower().Contains(SubName.ToLower()))
				.Where(a => Gender == null || a.Gender.ToLower().Equals(Gender.ToLower()))
				.Where(a => Address == null || a.Address.ToLower().Contains(Address.ToLower()))
				.Where(a => ExperienceYears == null || a.ExperienceYears == ExperienceYears)
				.Where(a => AppliedPosition == null || a.AppliedPosition == AppliedPosition)
				.Where(a => ((AppliedDateFrom == null && AppliedDateTo == null) ||
					(a.AppliedDate >= AppliedDateFrom && a.AppliedDate <= AppliedDateTo)))
				.Where(a => AskingSalary == null || a.AskingSalary == AskingSalary)
				.Select(a => new ApplicantDto
				{
					NationalId = a.NationalId,
					FullName = a.FullName,
					Gender = a.Gender,
					BirthDate = a.BirthDate,
					Address = a.Address,
					Phone = a.Phone,
					Email = a.Email,
					ExperienceYears = a.ExperienceYears,
					AppliedPosition = a.AppliedPosition,
					AppliedDate = a.AppliedDate,
					AskingSalary = a.AskingSalary,
				});

			var pagedApplicantDtos = await queryableFilteredApplicantDtos
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var totalCount = await queryableFilteredApplicantDtos.CountAsync();

			return new PagedResult<ApplicantDto>(
				items: pagedApplicantDtos, 
				totalCount: totalCount,
				page: page, 
				pageSize: pageSize);

        }

		public async Task<ApplicantDto?> GetApplicantAsync(string NationalId)
		{
			var applicant = await _context.Applicant
				.SingleAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return null;
			}

			return new ApplicantDto
			{
				NationalId = applicant.NationalId,
				FullName = applicant.FullName,
				Gender = applicant.Gender,
				BirthDate = applicant.BirthDate,
				Address = applicant.Address,
				Phone = applicant.Phone,
				Email = applicant.Email,
				ExperienceYears = applicant.ExperienceYears,
				AppliedPosition = applicant.AppliedPosition,
				AppliedDate = applicant.AppliedDate,
				AskingSalary = applicant.AskingSalary,
			};
		}

		public async Task<bool> AddApplicantAsync(ApplicantDto applicantDto)
		{
			await _validator.ValidateAndThrowAsync(applicantDto);

			//var duplicateApplicant = await _context.Applicant
			//	.SingleAsync(a => a.NationalId == applicantDto.NationalId);

			//if (duplicateApplicant != null)
			//{
			//	return false;
			//}

			var applicant = new Applicant
			{
				NationalId = applicantDto.NationalId,
				FullName = applicantDto.FullName,
				Gender = applicantDto.Gender,
				BirthDate = applicantDto.BirthDate,
				Address = applicantDto.Address,
				Phone = applicantDto.Phone,
				Email = applicantDto.Email,
				ExperienceYears = applicantDto.ExperienceYears,
				AppliedPosition = applicantDto.AppliedPosition,
				AppliedDate = applicantDto.AppliedDate,
				AskingSalary = applicantDto.AskingSalary,
			};
			await _context.Applicant.AddAsync(applicant);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> UpdateApplicantAsync(string NationalId, ApplicantDto applicantDto)
		{
			var applicant = await _context.Applicant
				.SingleAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return false;
			}

			applicant.NationalId = applicantDto.NationalId;
			applicant.FullName = applicantDto.FullName;
			applicant.Gender = applicantDto.Gender;
			applicant.BirthDate = applicantDto.BirthDate;
			applicant.Address = applicantDto.Address;
			applicant.Phone = applicantDto.Phone;
			applicant.Email = applicantDto.Email;
			applicant.ExperienceYears = applicantDto.ExperienceYears;
			applicant.AppliedPosition = applicantDto.AppliedPosition;
			applicant.AppliedDate = applicantDto.AppliedDate;
			applicant.AskingSalary = applicantDto.AskingSalary;

			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteApplicantsAsync()
		{
			var applicants = await _context.Applicant.ToListAsync();
			_context.Applicant.RemoveRange(applicants);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteApplicantAsync(string NationalId)
		{
			var applicant = await _context.Applicant
				.SingleAsync(a => a.NationalId == NationalId);

			if (applicant == null)
			{
				return false;
			}

			_context.Applicant.Remove(applicant);
			await _context.SaveChangesAsync();

			return true;
		}
	}
}
