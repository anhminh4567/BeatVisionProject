using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repository.Interface;
using Services.Implementation;
using Services.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.RequestDto;
using Shared.ResponseDto;
using StackExchange.Redis;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageTrackController : ControllerBase
	{
		private readonly TrackManager _trackManager;
		private readonly AppsettingBinding _appsetting;
		private readonly IUserIdentityService _userIdentityService;
		private readonly AppUserManager _appUserManager;
		private readonly IUnitOfWork _unitOfWork;
		private readonly CommentService _commentService;
		private readonly int AmountPerPage = 10;

		public ManageTrackController(TrackManager trackManager, AppsettingBinding appsetting, IUserIdentityService userIdentityService, AppUserManager appUserManager, IUnitOfWork unitOfWork, CommentService commentService)
		{
			_trackManager = trackManager;
			_appsetting = appsetting;
			_userIdentityService = userIdentityService;
			_appUserManager = appUserManager;
			_unitOfWork = unitOfWork;
			_commentService = commentService;
		}

		[HttpGet("get-public-trackfile")]
		public async Task<ActionResult> GetTrackMp3Public([FromQuery] int trackId)
		{
			var getResult = await _trackManager.GetTrackMp3Public(trackId);
			if (getResult.isSuccess is false)
			{
				return StatusCode(getResult.Error.StatusCode, getResult.Error);
			}
			return File(getResult.Value.Stream, getResult.Value.ContentType, true);
		}
		[HttpGet("get-range")]
		public async Task<ActionResult<IList<PagingResponseDto<IList<TrackResponseDto>>>>> GetTrackRange([FromQuery] int currentPage, [FromQuery] int amount = 10)
		{
			var trueStartPosition = currentPage * amount;
			var amountToTake = amount;
			var totalTrackCount = _trackManager.GetTotalTrackCount();
			var response = new PagingResponseDto<IList<TrackResponseDto>>();
			response.TotalCount= totalTrackCount;
			response.Value = await _trackManager.GetTracksRange(trueStartPosition, amountToTake);
			return Ok( response);
		}
		[HttpGet("get-range-status")]
		public async Task<ActionResult<IList<PagingResponseDto<IList<TrackResponseDto>>>>> GetTrackRange([FromQuery] string TRACK_STATUS, [FromQuery] int currentPage, [FromQuery] int amount = 10)
		{
			var trueStartPosition = currentPage * amount;
			var amountToTake = amount;
			var totalTrackCount = _trackManager.GetTotalTrackCount();
			var response = new PagingResponseDto<IList<TrackResponseDto>>();
			response.TotalCount = totalTrackCount;
			response.Value = await _trackManager.GetTrackRange_Status(trueStartPosition, amountToTake, TRACK_STATUS);
			return Ok(response);
		}
		[HttpGet("get-detail/{trackId}")]
		public async Task<ActionResult<TrackResponseDto>> GetTrackDetail([FromRoute] int trackId)
		{
			return Ok(await _trackManager.GetTrackDetail(trackId));
		}
		[HttpGet("get-tracks-tags")]
		public async Task<ActionResult<IList<TrackResponseDto>>> GetTrackByTags([FromQuery] params int[] tagIds)
		{
			if (tagIds is null || tagIds.Length == 0)
			{
				return Ok(new List<TrackResponseDto>());
			}
			var getRelatedTrack = await _trackManager.GetTracksWithTags(tagIds);
			return Ok(getRelatedTrack);
		}
		[HttpPost]
		//[Consumes("multipart/form-data")]
		public async Task<ActionResult> CreateTrack([FromForm] CreateTrackDto createTrackDto, CancellationToken cancellationToken = default)
		{
			var uploadResult = await _trackManager.UploadTrack(createTrackDto, cancellationToken);
			if (uploadResult.isSuccess is false)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
			return Ok("upload success");
		}
		[HttpPost("publish-track")]
		public async Task<ActionResult> PublishTrack([FromForm] PublishTrackDto publishTrackDto)
		{
			//var publishDateUtc = DateTime.SpecifyKind(publishTrackDto.PublishDate, DateTimeKind.Utc);
			//var publishDateLocal = TimeZoneInfo.ConvertTimeFromUtc(publishDateUtc, TimeZoneInfo.Local);
			var setResult = await _trackManager.SetPublishTrack(publishTrackDto);
			if (setResult.isSuccess is false)
			{
				return StatusCode(setResult.Error.StatusCode, setResult.Error);
			}
			return Ok();
		}
		[HttpDelete("pulldown-track/{id}")]
		public async Task<ActionResult> RemoveTrackFromPublish(int id)
		{
			var pulldownResult = await _trackManager.PulldownTrack(id);
			if (pulldownResult.isSuccess is false)
			{
				return StatusCode(pulldownResult.Error.StatusCode, pulldownResult.Error);
			}
			return Ok();
		}
		[HttpGet("get-comments")]
		public async Task<ActionResult> GetTrackComments([FromQuery] int trackId)
		{
			var getComments = await _trackManager.GetTrackComments(trackId);
			return Ok(getComments);
		}
		[HttpGet("get-comments-reply")]
		public async Task<ActionResult> GetTrackCommentReplies([FromQuery] int trackId, [FromQuery] int commentId)
		{
			var getComments = await _trackManager.GetTrackCommentReplies(trackId, commentId);
			return Ok(getComments);
		}
		[HttpGet("get-track-license")]
		public async Task<ActionResult> GetTrackLicense([FromQuery]int licenseId)
		{
			var getResult = await _trackManager.GetTrackLicense(licenseId);
			return Ok(getResult);
		}
		[HttpGet("get-track-license-paging")]
		public async Task<ActionResult<PagingResponseDto<IList<TrackLicenseDto>>>> GetTrackLicensePaging([FromQuery] int start, int amount)
		{
			if (start == null || start < 0)
				return BadRequest();
			var trueStart = start * amount;
			var getResult = await _trackManager.GetTrackLicensePaging(trueStart, amount);
			var getTotalTrackCount = _trackManager.GetTrackLicenseCount();
			return Ok(new PagingResponseDto<IList<TrackLicenseDto>>()
			{
				TotalCount = getTotalTrackCount,
				Value = getResult
			}) ;
		}
		[HttpGet("download-track-license")]
		public async Task<ActionResult> DownloadTrackLicensePaging([FromQuery] int licenseId)
		{
			var downloadResult = await _trackManager.DownloadTrackLicense(licenseId);
			if(downloadResult.isSuccess is false)
			{
				return StatusCode(downloadResult.Error.StatusCode,downloadResult.Error);
			}
			return File(downloadResult.Value.Stream,downloadResult.Value.ContentType);
		}
		[HttpPost("add-license")]
		public async Task<ActionResult> AddTrackLicense([FromForm] CreateTrackLicenseDto createTrackLicenseDto, CancellationToken cancellationToken = default)
		{
			var uploadedLicenseFile = createTrackLicenseDto.LicensePdfFile;
			using Stream fileStream = uploadedLicenseFile.OpenReadStream();
			var createresult = await _trackManager.AddTrackLicense(createTrackLicenseDto, fileStream, uploadedLicenseFile.FileName, ApplicationStaticValue.ContentTypePdf, cancellationToken);
			if (createresult.isSuccess is false)
			{
				return StatusCode(createresult.Error.StatusCode, createresult.Error);
			}
			return Ok(createresult.Value);
		}
		[HttpDelete("delete-license/{licenseId}")]
		public async Task<ActionResult> DeleteTrackLicense(int licenseId)
		{
			if (licenseId == null || licenseId <= 0)
			{
				return BadRequest();
			}
			var deleteResult = await _trackManager.RemoveTrackLicense(licenseId);
			if (deleteResult.isSuccess is false)
			{
				return StatusCode(deleteResult.Error.StatusCode, deleteResult.Error);
			}
			return Ok();
		}
		[HttpPost("add-track-to-license")]
		public async Task<ActionResult> AddTrackToLicense([FromForm]int trackId, [FromForm] int licenseId)
		{
			if (trackId <= 0 || licenseId <= 0)
			{
				return BadRequest();
			}
			var addResult = await _trackManager.AddTrackToLicense(trackId, licenseId);
			if(addResult.isSuccess is false)
				return StatusCode(addResult.Error.StatusCode,addResult.Error);
			return Ok();
		}
		[HttpDelete("delete-track-from-license")]
		public async Task<ActionResult> DeleteTrackFromLicense([FromQuery] int trackId, [FromQuery] int licenseId)
		{
			if (trackId <= 0 || licenseId <= 0)
			{
				return BadRequest();
			}
			var deleteResult = await _trackManager.RemoveTrackFromLicense(trackId,licenseId);
			if (deleteResult.isSuccess is false)
			{
				return StatusCode(deleteResult.Error.StatusCode, deleteResult.Error);
			}
			return Ok();
		}
	}
}
