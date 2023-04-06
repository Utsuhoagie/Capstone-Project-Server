﻿using Microsoft.AspNetCore.Identity;

namespace Capstone.Models
{
    public class EmployeeUser : IdentityUser
    {
        //public string UserName { get; set; } = string.Empty;
        //public string Password { get; set; } = string.Empty;

		public string? RefreshToken { get; set; }

		public DateTime RefreshTokenExpiryTime { get; set; }

		public Employee? Employee { get; set; }
		public int? EmployeeId { get; set; }
    }
}