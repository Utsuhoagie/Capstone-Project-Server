using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone.Features.FileModule
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
			var DANGEROUS_FILE_PATH = $"{_configuration.GetSection("FilePath").Value}\\{module}";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, imageFileName);
			var s = Path.ChangeExtension(safeFilePathName, "jpeg");
			return PhysicalFile(s, "image/jpeg"); 
		}

		[HttpGet("Document/{module}/{documentFileName}")]
		public IActionResult GetDocumentFromFileName(string module, string documentFileName)
		{
			var DANGEROUS_FILE_PATH = $"{_configuration.GetSection("FilePath").Value}\\{module}";
			var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, documentFileName);
			var s = Path.ChangeExtension(safeFilePathName, "pdf");
			return PhysicalFile(s, "application/pdf"); 
		}
	}
}
