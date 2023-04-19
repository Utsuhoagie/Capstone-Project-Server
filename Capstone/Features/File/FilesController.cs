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

		[HttpGet("Image/{module}/{imageFileName}")]
		public IActionResult GetImageFromFileName(string module, string imageFileName)
		{
			//var image = System.IO.
			var DANGEROUS_FILE_PATH = $"{_configuration.GetSection("FilePath").Value}\\{module}";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, imageFileName);
			var s = Path.ChangeExtension(safeFilePathName, "jpeg");
			return PhysicalFile(s, "image/jpeg"); 
		}
	}
}
