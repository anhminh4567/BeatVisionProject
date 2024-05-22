using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Services.Interface;
using Shared.Enums;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FileController : ControllerBase
	{
		private readonly FileService _fileService;

		public FileController(FileService fileService)
		{
			_fileService = fileService;
		}
		[HttpPost("upload-file")]
		public async Task<IActionResult> UploadFile(IFormFile file, BlobDirectoryType type,string filepath, CancellationToken cancellationToken = default)
		{
			using Stream filStream = file.OpenReadStream();
			var uploadResult =await _fileService.UploadFileAsync(filStream,  file.ContentType, filepath,type, cancellationToken);
			return Ok(uploadResult);
		}
		[HttpPost("download-file")]
		public async Task<IActionResult> DownloadFile(string fileId, BlobDirectoryType type, CancellationToken cancellationToken = default) 
		{
			var downloadResult = await _fileService.DownloadFileAsync(fileId,type,cancellationToken);
			return File(downloadResult.Value.Stream,downloadResult.Value.ContentType);
		}
		[HttpPost("delete-file")]
		public async Task<IActionResult> DeleteFile(string fileId, BlobDirectoryType type, CancellationToken cancellationToken = default)
		{
			var deleteResult = await _fileService.DeleteFileAsync(fileId, type);
			if (deleteResult.isSuccess is false)
				return StatusCode(deleteResult.Error.StatusCode, deleteResult.Error);
			return Ok();
		}
		[HttpPost("download-file-segment")]
		public IActionResult DownloadFileSegment(string furtherBreakdown, string fileId, CancellationToken cancellationToken = default)
		{
			return Ok();
			//var filepath = furtherBreakdown + "/" + fileId;
			//var testPath = "good.mp4";
		    //return _fileService.StreamFileSegmentAsync(testPath, HttpContext,cancellationToken);
		}
		[HttpPost("cloudinary-file")]
		public async Task<IActionResult> Test(IFormFile file)
		{
			Console.WriteLine(Directory.GetCurrentDirectory());
			//var newCloudinary = new AudioFileTransformationService();
			//await newCloudinary.upload(file);
			return Ok();
		}
	}
}
