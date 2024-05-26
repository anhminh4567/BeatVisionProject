using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Repository.Interface;
using Services.Implementation;
using Services.Interface;
using Shared.ConfigurationBinding;
using Shared.RequestDto;
using Shared.ResponseDto;

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

		public ManageTrackController(TrackManager trackManager, AppsettingBinding appsetting, IUserIdentityService userIdentityService, AppUserManager appUserManager, IUnitOfWork unitOfWork, CommentService commentService, int amountPerPage)
		{
			_trackManager = trackManager;
			_appsetting = appsetting;
			_userIdentityService = userIdentityService;
			_appUserManager = appUserManager;
			_unitOfWork = unitOfWork;
			_commentService = commentService;
			AmountPerPage = amountPerPage;
		}

		[HttpGet("get-public-trackfile")]
		public async Task<ActionResult> GetTrackMp3Public([FromQuery]int trackId)
		{
			var getResult = await _trackManager.GetTrackMp3Public(trackId);
			if(getResult.isSuccess is false)
			{
				return StatusCode(getResult.Error.StatusCode,getResult.Error);
			}
			return File(getResult.Value.Stream,getResult.Value.ContentType,true);
		}
		[HttpGet("get-range")]
		public async Task<ActionResult<IList<TrackResponseDto>>> GetTrackRange([FromQuery] int currentPage)
		{
			var trueStartPosition = currentPage * AmountPerPage;
			var amountToTake = AmountPerPage;
			return Ok(await _trackManager.GetTracksRange(trueStartPosition, amountToTake));
		}
		[HttpGet("get-detail/{trackId}")]
		public async Task<ActionResult<TrackResponseDto>> GetTrackDetail([FromRoute] int trackId)
		{
			return Ok(await _trackManager.GetTrackDetail(trackId));
		}
		[HttpGet("get-tracks-tags")]
		public async Task<ActionResult<IList<TrackResponseDto>>> GetTrackByTags([FromQuery]params int[] tagIds)
		{
			if(tagIds is null || tagIds.Length == 0)
			{
				return Ok(new List<TrackResponseDto>());
			}
			var getRelatedTrack = await _trackManager.GetTracksWithTags(tagIds);
			return Ok(getRelatedTrack);
		}
		[HttpPost]
		//[Consumes("multipart/form-data")]
		public async Task<ActionResult> CreateTrack([FromForm]CreateTrackDto createTrackDto, CancellationToken cancellationToken = default)
		{
			var uploadResult = await _trackManager.UploadTrack(createTrackDto,cancellationToken);
			if(uploadResult.isSuccess is false)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
			return Ok("upload success");
		}
		[HttpPost("publish-track")]
		public async Task<ActionResult> PublishTrack([FromForm]PublishTrackDto publishTrackDto)
		{
			var setResult = await _trackManager.SetPublishTrack(publishTrackDto);
			if(setResult.isSuccess is false)
			{
				return StatusCode(setResult.Error.StatusCode,setResult.Error);
			}
			return Ok();
		}
		[HttpDelete("pulldown-track/{id}")]
		public async Task<ActionResult> RemoveTrackFromPublish(int id)
		{
			var pulldownResult = await _trackManager.PulldownTrack(id);
			if(pulldownResult.isSuccess is false)
			{
				return StatusCode(StatusCodes.Status500InternalServerError,pulldownResult.Error);
			}
			return Ok();
		}
		[HttpGet("get-comments")]
		public async Task<ActionResult> GetTrackComments([FromQuery]int trackId)
		{
			var getComments = await _trackManager.GetTrackComments(trackId);
			return Ok(getComments);
		}
		[HttpGet("get-comments-reply")]
		public async Task<ActionResult> GetTrackCommentReplies([FromQuery] int trackId, [FromQuery] int commentId)
		{
			var getComments = await _trackManager.GetTrackCommentReplies(trackId,commentId);
			return Ok(getComments);
		}

	}
}
