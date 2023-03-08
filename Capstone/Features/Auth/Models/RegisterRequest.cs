﻿namespace Capstone.Features.Auth.Models
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
		public string PasswordConfirm { get; set; } = string.Empty;
    }
}
