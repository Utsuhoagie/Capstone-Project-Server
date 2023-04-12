using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone.Features.File
{
	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public FilesController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpGet("Image/{imageFileName}")]
		public IActionResult GetImageFromFileName(string imageFileName)
		{
			//var image = System.IO.
			var DANGEROUS_PATH = _configuration.GetSection("FilePath").Value;
			var safeFilePathName = Path.Combine(DANGEROUS_PATH, imageFileName);
			var s = Path.ChangeExtension(safeFilePathName, "jpeg");
			return PhysicalFile(s, "image/jpeg"); 
		}
	}
}
