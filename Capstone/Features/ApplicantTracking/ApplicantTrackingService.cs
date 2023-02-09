using Capstone.Data;
using Capstone.ExceptionHandling;
using Capstone.Features.ApplicantTracking;
using Capstone.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Capstone.Features.ApplicantTracking
{
    public class ApplicantTrackingService : IApplicantTrackingService
    {
        private readonly CapstoneContext _context;
		private readonly IValidator<ApplicantDto> _validator;

        public ApplicantTrackingService(
			CapstoneContext capstoneContext, 
			IValidator<ApplicantDto> validator)
        {
            _context = capstoneContext;
			_validator = validator;
        }

		private async Task<bool> ApplicantExistsAsync(int id)
		{
			return await _context.Applicant.AnyAsync(a => a.Id == id);
		}

        public async Task<IEnumerable<ApplicantDto>> GetApplicantsAsync()
        {
            return await _context.Applicant.Select(a => new ApplicantDto
			{
				NationalId = a.NationalId,
				FullName = a.FullName,
				Gender = a.Gender,
				BirthDate = a.BirthDate,
				Address = a.Address,
				Phone = a.Phone,
				Email = a.Email,
				AppliedPosition = a.AppliedPosition,
				AppliedDate = a.AppliedDate,
				AskingSalary = a.AskingSalary,
			}).ToListAsync();
        }

		public async Task<ApplicantDto?> GetApplicantAsync(int id)
		{
			var applicant = await _context.Applicant.FindAsync(id);

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
				AppliedPosition = applicant.AppliedPosition,
				AppliedDate = applicant.AppliedDate,
				AskingSalary = applicant.AskingSalary,
			};
		}

		public async Task<bool> AddApplicantAsync(ApplicantDto applicantDto)
		{
			await _validator.ValidateAndThrowAsync(applicantDto);

			var applicant = new Applicant
			{
				NationalId = applicantDto.NationalId,
				FullName = applicantDto.FullName,
				Gender = applicantDto.Gender,
				BirthDate = applicantDto.BirthDate,
				Address = applicantDto.Address,
				Phone = applicantDto.Phone,
				Email = applicantDto.Email,
				AppliedPosition = applicantDto.AppliedPosition,
				AppliedDate = applicantDto.AppliedDate,
				AskingSalary = applicantDto.AskingSalary,
			};
			await _context.Applicant.AddAsync(applicant);
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

		public async Task<bool> DeleteApplicantAsync(int id)
		{
			var applicant = await _context.Applicant.FindAsync(id);

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
