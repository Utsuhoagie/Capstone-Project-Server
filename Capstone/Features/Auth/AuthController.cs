using Capstone.Features.Auth.Models;
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

		[HttpPost("Register")]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterRequest req)
		{
			if (req == null)
			{
				return BadRequest();
			}

			var registerResponse = await _service.Register(req);

			if (registerResponse.Status != HttpStatusCode.Created)
			{
				return BadRequest(registerResponse);
			}

			return Ok(registerResponse);
		}

		[HttpPost(template: "Login")]
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
	}
}
