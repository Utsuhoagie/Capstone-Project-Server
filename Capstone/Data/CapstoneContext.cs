﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Capstone.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Capstone.Data
{
    public class CapstoneContext : IdentityDbContext<EmployeeUser>
    {
        public CapstoneContext (DbContextOptions<CapstoneContext> options)
            : base(options)
        {
        }

		public DbSet<Person> People { get; set; } = default!;
		//public DbSet<Applicant> Applicants { get; set; } = default!;
		//public DbSet<Employee> Employees { get; set; } = default!;

		public DbSet<Attendance> Attendances { get; set; } = default!;

		public DbSet<Position> Positions { get; set; } = default!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Auth
			modelBuilder.Entity<IdentityRole>()
				.HasData(
				new IdentityRole
				{
					Id = "2f89c3c2-0e18-4919-9ee5-136ccb50f78a",
					Name = "Employee",
					NormalizedName = "EMPLOYEE",
					ConcurrencyStamp = "4b63da43-5bed-4afa-b24b-6cf71eb4f44a",
				},
				new IdentityRole
				{
					Id = "a1fcf63a-beb7-429b-a915-4f36bccfce18",
					Name = "Admin",
					NormalizedName = "ADMIN",
					ConcurrencyStamp = "9f791f97-52df-4f2d-9554-fc3e0ff8bde8",
				});


			// App
			modelBuilder.Entity<Person>()
				.Property(p => p.BirthDate)
				.HasColumnType("datetimeoffset");

			modelBuilder.Entity<Applicant>()
				.Property(a => a.AppliedDate)
				.HasColumnType("datetimeoffset");
			modelBuilder.Entity<Applicant>()
				.HasOne(a => a.AppliedPosition)
				.WithMany(p => p.Applicants)
				.HasForeignKey(a => a.AppliedPositionId)
				.IsRequired()
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<Employee>()
				.Property(e => e.EmployedDate)
				.HasColumnType("datetimeoffset");
			modelBuilder.Entity<Employee>()
				.HasOne(e => e.Position)
				.WithMany(p => p.Employees)
				.HasForeignKey(e => e.PositionId)
				.IsRequired()
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Employee>()
				.HasOne(e => e.User)
				.WithOne(u => u.Employee)
				.HasForeignKey<EmployeeUser>(u => u.EmployeeId);

			modelBuilder.Entity<Attendance>()
				.HasOne(a => a.Employee)
				.WithMany(e => e.Attendances)
				.HasForeignKey(a => a.EmployeeId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
		}
    }
}
