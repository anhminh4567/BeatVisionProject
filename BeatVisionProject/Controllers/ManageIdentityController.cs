using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Services.Implementation;
using Services.Interface;
using Shared.IdentityConfiguration;
using Shared.RequestDto;
using Shared;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography.Pkcs;
using Shared.Models;
using Shared.Enums;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageIdentityController : ControllerBase
	{
		private readonly AppUserManager _manageUserService;
		private readonly UserIdentityServices _userIdentityService;

		public ManageIdentityController(AppUserManager manageUserService, UserIdentityServices userIdentityService)
		{
			_manageUserService = manageUserService;
			_userIdentityService = userIdentityService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDto registerDto, CancellationToken cancellationToken = default)
		{
			var registerResult = await _userIdentityService.Register(registerDto, cancellationToken);
			if (registerResult.isSuccess is false)
			{
				return StatusCode(registerResult.Error.StatusCode, registerResult.Error);
			}

			return Ok(registerResult.Value);
		}
		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken = default)
		{
			var loginResult = await _userIdentityService.Login(loginDto, cancellationToken);
			if (loginResult.isSuccess is false)
			{
				return StatusCode(loginResult.Error.StatusCode, loginResult.Error);
			}
			return Ok(loginResult.Value);
		}
		[HttpPost("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			var userIdFromClaim = User.Claims.FirstOrDefault(c => c.Type.Equals(ApplicationStaticValue.UserIdClaimType))?.Value;
			if (userIdFromClaim == null)
				return Unauthorized();
			var getUser = await _userIdentityService.UserManager.FindByIdAsync(userIdFromClaim);
			if (getUser == null)
				return NotFound();
			var logoutResult = await _userIdentityService.Logout(getUser);
			if (logoutResult.isSuccess is false)
			{
				return StatusCode(logoutResult.Error.StatusCode, logoutResult.Error);
			}
			return Ok();
		}
		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([NotNull] string refreshToken)
		{
			if (string.IsNullOrEmpty(Request.Headers.Authorization))
				return BadRequest("no access token found");
			var accessToken = Request.Headers.Authorization.ToString().Substring(7);
			var generateResult = await _userIdentityService.Refresh(accessToken, refreshToken);
			if (generateResult.isSuccess is false)
			{
				return StatusCode(generateResult.Error.StatusCode, generateResult.Error);
			}
			return Ok(generateResult.Value);
		}
		[HttpGet("external-login")]
		public async Task<IActionResult> ExternalLogin(string providerSchemeName)
		{
			var externalLoginResult = await _userIdentityService.ExternalLogin(providerSchemeName, Url.Action(nameof(ExternalAuthenticationResponse)));
			if (externalLoginResult.isSuccess is false)
				return StatusCode(externalLoginResult.Error.StatusCode, externalLoginResult.Error);
			return externalLoginResult.Value;
		}
		[HttpGet("external-callback-url")]
		public async Task<IActionResult> ExternalAuthenticationResponse()
		{
			var info = await _userIdentityService.SigninManager.GetExternalLoginInfoAsync();
			if (info == null)
				return BadRequest();
			if( (info.Principal.Claims.FirstOrDefault(c => c.Type == "FAILURE") is not null))
			{ 
				return BadRequest("Cannot call request to user infor in google , try again later");
			}
			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			var result = await _userIdentityService.SigninManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
			if (result.Succeeded)
			{
				var getUserByEmail = await _userIdentityService.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				var tokenResult = await _userIdentityService.GenerateTokensForUsers(getUserByEmail);
				if (tokenResult.isSuccess is false)
					return StatusCode(tokenResult.Error.StatusCode, tokenResult.Error);
				return Ok(tokenResult.Value);
			}
			else
			{
				var googleName = info.Principal.FindFirstValue(ApplicationStaticValue.UsernameClaimType);
				var googleProfileImage = info.Principal.FindFirstValue(ApplicationStaticValue.ProfileImageUrlClaimType);
				var newUser = new CustomIdentityUser()
				{
					Email = email,
					UserName = email,
					Dob = null,
					SecurityStamp = Guid.NewGuid().ToString(),
					ConcurrencyStamp = Guid.NewGuid().ToString(),
					EmailConfirmed = true,
				};
				var newUserProfile = new UserProfile()
				{
					AccountStatus = AcccountStatus.ACTIVE,
					TotalAlbumn = 0,
					TotalTrack = 0,
					Fullname = googleName,
					ProfileBlobUrl = googleProfileImage,
				};
				var createResult = await _userIdentityService.CreateUser(newUser, newUserProfile);
				if(createResult.isSuccess is false)
				{
					return StatusCode(createResult.Error.StatusCode, createResult.Error);
				}
				return Ok(createResult.Value);
			}
		}
		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
		{
			var userIdFromClaim = User.Claims.FirstOrDefault(c => c.Type.Equals(ApplicationStaticValue.UserIdClaimType))?.Value;
			if (userIdFromClaim == null)
				return Unauthorized();
			var getUser = await _userIdentityService.UserManager.FindByIdAsync(userIdFromClaim);
			var changePasswordResult = await _userIdentityService.ChangePassword(getUser, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
			if (changePasswordResult.isSuccess is false)
			{
				return StatusCode(changePasswordResult.Error.StatusCode, changePasswordResult.Error);
			}
			await _userIdentityService.Logout(getUser);
			return Ok();
		}
		
		[HttpGet("send-confirmation-email")]
		[Authorize]
		public async Task<IActionResult> SendConfirmEmail(CancellationToken cancellationToken = default)
		{
			var userIdFromClaim = User.Claims.FirstOrDefault(c => c.Type.Equals(ApplicationStaticValue.UserIdClaimType))?.Value;
			if (userIdFromClaim == null)
				return Unauthorized();
			var getUser = await _userIdentityService.UserManager.FindByIdAsync(userIdFromClaim);
			var actionPath = Url.Action(nameof(EmailConfirmationDestination));
			var scheme = HttpContext.Request.Scheme;
			var host = HttpContext.Request.Host;
			var fullUrl = $"{scheme}://{host}{actionPath}";
			var sendResult = await _userIdentityService.SendConfirmEmail(getUser, fullUrl, cancellationToken);
			if (sendResult.isSuccess is false)
			{
				return StatusCode(sendResult.Error.StatusCode, sendResult.Error);
			}
			return Ok();
		}
		[HttpGet("confirm-email-callback")]
		public async Task<IActionResult> EmailConfirmationDestination(string userId, string token)
		{
			var confirmResult = await _userIdentityService.ConfirmEmailToken(userId, token);
			if (confirmResult.isSuccess is false)
				return StatusCode(confirmResult.Error.StatusCode, confirmResult.Error);
			return Ok("email confirmed");
		}
		[HttpGet("get-user")]
		public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken = default)
		{
			//var result = await _unitOfWork.Repositories.customIdentityUser.GetById(id);
			return Ok();
		}
	}
}
