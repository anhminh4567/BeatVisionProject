using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.RequestDto;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageCommentController : ControllerBase
	{
		private readonly AppUserManager _appUserManager;
		private readonly TrackManager _trackManager;
		private readonly AppsettingBinding _appsettings;
		private readonly CommentService _commentService;

		public ManageCommentController(AppUserManager userManager, TrackManager trackManager, AppsettingBinding appsettings, CommentService commentService)
		{
			_appUserManager = userManager;
			_trackManager = trackManager;
			_appsettings = appsettings;
			_commentService = commentService;
		}
		[HttpGet("get-user-track-comment")]
		public async Task<ActionResult> GetUserTrackComment([FromQuery] int userId)
		{
			var getUserComments = await _appUserManager.GetUserTrackComments(userId);
			if (getUserComments.isSuccess is false)
				return StatusCode(getUserComments.Error.StatusCode,getUserComments.Error);
			return Ok(getUserComments.Value);
		}
		[HttpGet("get-track-comments")]
		public async Task<ActionResult> GetTrackComments([FromQuery] int trackId)
		{
			var getComments = await _trackManager.GetTrackComments(trackId);
			return Ok(getComments);
		}
		[HttpGet("get-track-comments-reply")]
		public async Task<ActionResult> GetTrackCommentReplies([FromQuery] int trackId, [FromQuery] int commentId)
		{
			var getComments = await _trackManager.GetTrackCommentReplies(trackId, commentId);
			return Ok(getComments);
		}
		[HttpPost("create-track-command")]
		public async Task<ActionResult> CreateUserCommand([FromForm] int userProfileId, [FromForm] CreateTrackCommentDto createTrackCommentDto)
		{
			createTrackCommentDto.ReplyToCommentId = null;
			var createResult = await _appUserManager.CreateComment(userProfileId, createTrackCommentDto);
			if (createResult.isSuccess is false)
				return StatusCode(createResult.Error.StatusCode, createResult.Error);
			return Ok(createResult.Value);
		}
		[HttpPost("create-track-command-reply")]
		public async Task<ActionResult> CreateUserCommandReply([FromForm] int userProfileId, [FromForm] CreateTrackCommentDto createTrackCommentDto)
		{
			var createResult = await _appUserManager.CreateCommentReply(userProfileId, createTrackCommentDto);
			if (createResult.isSuccess is false)
				return StatusCode(createResult.Error.StatusCode, createResult.Error);
			return Ok(createResult.Value);
		}
		[HttpDelete("remove-user-comment")]
		public async Task<ActionResult> RemoveUserComment([FromForm]RemoveCommentDto removeCommentDto)
		{
			var removeResult = await _appUserManager.RemoveComment(removeCommentDto.UserProfileId,removeCommentDto.CommentId);
			if(removeResult.isSuccess is false)
				return StatusCode(removeResult.Error.StatusCode,removeResult.Error);
			return Ok();
		}
		//[HttpPut("like-comment")]
		//public async Task<ActionResult> LikeComment([FromQuery] int commentId)
		//{
		//	var updateResult = await _commentService.up
		//	if (removeResult.isSuccess is false)
		//		return StatusCode(removeResult.Error.StatusCode, removeResult.Error);
		//	return Ok();
		//}
	}
}
