using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using FluentValidation;
using Capstone.Data;
using Capstone.Models;
using Capstone.Features;
using Capstone.Features.ApplicantModule;
using Capstone.Features.EmployeeModule;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Capstone.Features.Auth;
using Capstone.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Capstone.Features.PositionModule;
using Capstone.Responses.ExceptionHandling;
using Microsoft.AspNetCore.Http.Features;
using Capstone.Features.AttendanceModule;

var builder = WebApplication.CreateBuilder(args);

// === Add Services to the container.

builder.Services.AddDbContext<CapstoneContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("CapstoneContext")
		?? throw new InvalidOperationException("Connection string 'CapstoneContext' not found.")
	)
);

#region---- Auth ----
// NOTE: AddIdentity MUST go before AddAuthentication
// Because it has its own AddAuthentication that uses Cookies, not JWT
builder.Services
	.AddIdentity<EmployeeUser, IdentityRole>(options =>
	{
		options.User.RequireUniqueEmail = true;
		options.Password.RequiredLength = 8;
		options.Password.RequireUppercase = false;
		options.Password.RequireNonAlphanumeric = false;
	})
	.AddEntityFrameworkStores<CapstoneContext>();

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero,
			ValidIssuer = builder.Configuration.GetSection("JWT:ValidIssuer").Value,
			ValidAudiences = builder.Configuration.GetSection("JWT:ValidAudiences").Get<string[]>(),
			//ValidAudience = builder.Configuration.GetSection("JWT:ValidAudience").Value,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				builder.Configuration.GetSection("JWT:SecretKey").Value
			))
		};
	});

builder.Services
	.AddAuthorization(options =>
	{
		//options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
		//	.RequireAuthenticatedUser()
		//	.Build();
	});

builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
#endregion

#region---- General ----
builder.Services
	.AddControllers(options =>
	{
		options.Filters.Add<HttpResponseExceptionFilter>();
	})
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
	});

builder.Services
	.AddCors(p =>
	p.AddPolicy("Capstone", b =>
		b.WithOrigins(new[] { "http://localhost:3000", "http://localhost:19000" })
		 .AllowAnyHeader()
		 .AllowAnyMethod()
	)
);
builder.Services.Configure<FormOptions>(options =>
{
	//options.MemoryBufferThreshold = 20 * 1024 * 1024;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Auth Service ----
builder.Services.AddScoped<IAuthService, AuthService>();

// ---- App Services ----
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// ---- Validation Services ----
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddScoped<IValidator<ApplicantDto>, ApplicantValidator>();
builder.Services.AddScoped<IValidator<EmployeeDto>, EmployeeValidator>();
builder.Services.AddScoped<IValidator<PositionDto>, PositionValidator>();
builder.Services.AddScoped<IValidator<AttendanceDto>, AttendanceValidator>();
#endregion

// === End Services Config.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Capstone");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
