using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using FluentValidation;

using Capstone.ExceptionHandling;
using Capstone.Data;
using Capstone.Models;
using Capstone.Features;
using Capstone.Features.ApplicantModule;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CapstoneContext>(options =>
    options.UseSqlServer(
		builder.Configuration.GetConnectionString("CapstoneContext") 
		?? throw new InvalidOperationException("Connection string 'CapstoneContext' not found.")
	)
);

// === Add Services to the container.

// ---- AUTH ----
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//	.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// ---- General ----
builder.Services.AddControllers(options => 
	{
		options.Filters.Add<HttpResponseExceptionFilter>();
	})
	.AddJsonOptions(options => { 
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
	});
builder.Services.AddCors(p => 
	p.AddPolicy("Capstone", b => 
		b.WithOrigins("http://localhost:3000")
		 .AllowAnyHeader()
		 .AllowAnyMethod()
	)
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- App Services ----
builder.Services.AddScoped<IApplicantService, ApplicantService>();


// ---- Validation Services ----
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IValidator<ApplicantDto>, ApplicantValidator>();

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

//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
