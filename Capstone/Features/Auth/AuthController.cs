using Capstone.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Capstone.Features.Auth
{
    [Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _service;
		public AuthController(IAuthService service) 
		{
			_service = service;
		}

		[HttpPost("RegisterEmployee")]
		public async Task<IActionResult> RegisterEmployee(RegisterEmployeeRequest req)
		{
			if (req == null)
			{
				return BadRequest();
			}

			var registerEmployeeResponse = await _service.RegisterEmployee(req);

			if (registerEmployeeResponse.Status != HttpStatusCode.OK)
			{
				return StatusCode((int)registerEmployeeResponse.Status, registerEmployeeResponse);
			}

			return Ok(registerEmployeeResponse);
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login(LoginRequest req)
		{
			if (req == null)
			{
				return BadRequest();
			}

			var loginResponse = await _service.Login(req);

			if (loginResponse.Status != HttpStatusCode.OK)
			{
				return StatusCode((int)loginResponse.Status, loginResponse);
			}

			return Ok(loginResponse);
		}

		[HttpPost("Refresh")]
		//[Authorize]
		public async Task<IActionResult> Refresh(RefreshRequest req)
		{
			var result = await _service.Refresh(req);

			if (result.Status != HttpStatusCode.OK)
			{
				return StatusCode((int)result.Status, result);
			}

			return Ok(result);
		}

		[HttpGet("CheckToken")]
		[Authorize]
		public async Task<IActionResult> CheckToken()
		{
			return Ok();
		}

		// ========================
		// FOR DEBUGGING ONLY!!!!!!

		[HttpPost("DEBUG_REGISTER")]
		public async Task<IActionResult> DEBUG_REGISTER(RegisterRequest req)
		{
			if (req == null)
			{
				return BadRequest();
			}

			var registerResponse = await _service.DEBUG_REGISTER(req);

			if (registerResponse.Status != HttpStatusCode.Created)
			{
				return BadRequest(registerResponse);
			}

			return Ok(registerResponse);
		}

		[HttpDelete("DEBUG_DELETE/{email}")]
		public async Task<IActionResult> DEBUG_DELETE(string email)
		{
			return Ok(await _service.DEBUG_DELETE(email));
		}
	}
}
