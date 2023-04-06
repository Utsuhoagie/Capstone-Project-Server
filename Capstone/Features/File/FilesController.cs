using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Capstone.Features.File
{
	public class FileUploadRequest
	{
		public IFormFile File1 { get; set; } = default!;
	}

	public class UploadAttendanceRequest
	{
		public DateTimeOffset Timestamp { get; set; }
		public string Hash { get; set; } = string.Empty;
		public IFormFile Image { get; set; } = default!;
	}


	[Route("api/[controller]")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly string DANGEROUS_FILE_PATH = "E:\\Study\\HCMUT\\Capstone\\App\\server 2\\DANGEROUS_FILES";
		private readonly string DAILY_HASH = "abc";

		[HttpPost("Upload")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> Upload([FromForm] FileUploadRequest req)
		{
			var file1 = req.File1;
			if (file1.Length > 0)
			{
				var safeFileName = Path.GetRandomFileName();
				var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, safeFileName);
				var safeFilePathNameWithCorrectExtension = Path.ChangeExtension(safeFilePathName, "jpg");

				using (var fileStream = System.IO.File.Create(safeFilePathNameWithCorrectExtension))
				{
					await file1.CopyToAsync(fileStream);
				}
			}
			return Ok(new
			{
				Message = "File is uploaded",
				File = file1
			});
		}

		[HttpPost("Attendance")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> Attendance([FromForm] UploadAttendanceRequest req)
		{
			var image = req.Image;
			if (image.Length > 0)
			{
				var safeFileName = Path.GetRandomFileName();
				var safeFilePathName = Path.Combine(DANGEROUS_FILE_PATH, safeFileName);

				using (var fileStream = System.IO.File.Create(safeFilePathName))
				{
					await image.CopyToAsync(fileStream);
				}
			}
			return Ok(new
			{
				Message = $"File is uploaded at {req.Timestamp}. Hash match? {req.Hash == DAILY_HASH}",
				Image = image
			});
		}

		[HttpGet("Test")]
		public IActionResult Test()
		{
			return Ok("test");
		}

		// GET api/Files/5
		[HttpGet]
		public string Get()
		{
			return "value";
		}

		// POST api/<FilesController>
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/<FilesController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<FilesController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
