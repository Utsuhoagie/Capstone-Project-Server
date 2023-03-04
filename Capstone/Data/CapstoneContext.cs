using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Capstone.Models;

namespace Capstone.Data
{
    public class CapstoneContext : DbContext
    {
        public CapstoneContext (DbContextOptions<CapstoneContext> options)
            : base(options)
        {
        }

		public DbSet<Person> People { get; set; } = default!;
        //public DbSet<Applicant> Applicants { get; set; } = default!;
		//public DbSet<Employee> Employees { get; set; } = default!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Person>()
				.Property(p => p.BirthDate)
				.HasColumnType("datetimeoffset");

			modelBuilder.Entity<Applicant>()
				.Property(a => a.AppliedDate)
				.HasColumnType("datetimeoffset");

			modelBuilder.Entity<Employee>()
				.Property(e => e.EmployedDate)
				.HasColumnType("datetimeoffset");
		}
    }
}
