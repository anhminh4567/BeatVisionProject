using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.Helper;
using Shared.RequestDto;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageUserController : ControllerBase
	{
		private readonly AppUserManager _appUserManager;
		private readonly AppsettingBinding _appsettings;

		public ManageUserController(AppUserManager appUserManager, AppsettingBinding appsettings)
		{
			_appUserManager = appUserManager;
			_appsettings = appsettings;
		}
		[HttpGet("{id}")]
		public async Task<ActionResult> GetUserProfile(int profileId)
		{
			var getProfile = await _appUserManager.GetUserProfile(profileId);
			if (getProfile is null)
				return BadRequest();
			return Ok();
		}
		[HttpGet("identity/{identityId}")]
		public async Task<ActionResult> GetUserProfileByIdentity(int identityId)
		{
			var getProfile = await _appUserManager.GetUserProfileByIdentity(identityId);
			if (getProfile is null)
				return BadRequest();
			return Ok();
		}
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateUserProfile([FromRoute] int id,[FromBody] UpdateUserProfileDto updateUserProfileDto)
		{
			var getUserProfile = await _appUserManager.GetUserProfile(id);
			if(getUserProfile is null)
			{
				return BadRequest();
			}
			var updateResult = await _appUserManager.UpdateProfile(getUserProfile,updateUserProfileDto);
			if(updateResult.isSuccess is false)
			{
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			}
			return Ok(updateResult.Value);
		}

		[HttpPut("profile-image/{id}")]
		public async Task<ActionResult> UpdateProfileImage(int id,UpdateProfileImageDto updateProfileImageDto)
		{
			var getFile = updateProfileImageDto.imageFile;
			var getProfile = await _appUserManager.GetUserProfile(id);
			var fileName = updateProfileImageDto.imageFile.FileName;
			var getExtension = FileHelper.ExtractFileExtention(fileName);
			if(getExtension.isSuccess is false)
			{
				return StatusCode(getExtension.Error.StatusCode, getExtension.Error);
			}
			var isExtensionAllowed = FileHelper.IsExtensionAllowed(getExtension.Value, _appsettings.AppConstraints.AllowImageExension);
			if(isExtensionAllowed.isSuccess is false)
			{
				return StatusCode(isExtensionAllowed.Error.StatusCode, isExtensionAllowed.Error);
			}
			using Stream imageStream = getFile.OpenReadStream();
			var updateResult = await _appUserManager.UpdateProfileImage(imageStream, getFile.ContentType,getFile.FileName,getProfile);
			if(updateResult.isSuccess is false)
			{
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			}
			return Ok();
		}
		[HttpGet("get-track-comment")]
		public async Task<ActionResult> GetUserTrackComment([FromQuery]int userId)
		{
			var getuserProfile = await _appUserManager.GetUserProfile(userId);
			var getUserComments = await _appUserManager.GetUserTrackComments(getuserProfile);
			return Ok(getUserComments);
		}
	}

}
