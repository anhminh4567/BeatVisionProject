using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Services.Implementation;
using Shared;
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
		[HttpGet("{profileId}")]
		public async Task<ActionResult> GetUserProfile([FromRoute]int profileId)
		{
			var getProfile = await _appUserManager.GetUserProfile(profileId);
			if (getProfile is null)
				return BadRequest();
			return Ok(getProfile);
		}
		[HttpGet("identity/{identityId}")]
		public async Task<ActionResult> GetUserProfileByIdentity([FromRoute] int identityId)
		{
			var getProfile = await _appUserManager.GetUserProfileByIdentity(identityId);
			if (getProfile is null)
				return BadRequest();
			return Ok(getProfile);
		}
		[HttpPut("{profileId}")]
		public async Task<ActionResult> UpdateUserProfile([FromRoute] int profileId, [FromBody] UpdateUserProfileDto updateUserProfileDto)
		{
			var updateResult = await _appUserManager.UpdateProfile(profileId, updateUserProfileDto);
			if(updateResult.isSuccess is false)
			{
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			}
			return Ok(updateResult.Value);
		}

		[HttpPut("profile-image/{id}")]
        //[Authorize(policy: ApplicationStaticValue.USER_POLICY_NAME)]
        public async Task<ActionResult> UpdateProfileImage([FromRoute] int id,[FromForm]UpdateProfileImageDto updateProfileImageDto)
		{
			var getFile = updateProfileImageDto.imageFile;
			//var getProfile = await _appUserManager.GetUserProfile(id);
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
			var updateResult = await _appUserManager.UpdateProfileImage(imageStream, getFile.ContentType,getFile.FileName,id);
			if(updateResult.isSuccess is false)
			{
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			}
			return Ok();
		}
		
		[HttpGet("subscribe")]
        //[Authorize(policy: ApplicationStaticValue.USER_POLICY_NAME)]
        public async Task<ActionResult> Subscribe([FromQuery] int userId)
		{
			var subscribeResult = await _appUserManager.Subscribe(userId);
			if (subscribeResult.isSuccess is false)
				return StatusCode(subscribeResult.Error.StatusCode,subscribeResult.Error);
			return Ok(subscribeResult.Value);
		}
	}

}
