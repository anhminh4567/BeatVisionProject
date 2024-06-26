﻿using Microsoft.AspNetCore.Authorization;
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
using System.Runtime.CompilerServices;
using Shared.ConfigurationBinding;
using System.Threading;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageIdentityController : ControllerBase
	{
		private readonly AppUserManager _manageUserService;
		private readonly IUserIdentityService _userIdentityService;
		private readonly AppsettingBinding _appsettings;
		private readonly int PAGING_TAKE_LIMIT;
		public ManageIdentityController(AppUserManager manageUserService, IUserIdentityService userIdentityService, AppsettingBinding appsetting)
		{
			_manageUserService = manageUserService;
			_userIdentityService = userIdentityService;
			_appsettings = appsetting;
			PAGING_TAKE_LIMIT = _appsettings.AppConstraints.PagingTakeLimit;
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
		[HttpPost("register-admin")]
		//[Authorize(policy: ApplicationStaticValue.ADMIN_POLICY_NAME)]
		public async Task<ActionResult> RegisterAdmin(RegisterDto registerDto)
		{
			var registerResult = await _userIdentityService.CreateAdmin(registerDto);
			if (registerResult.isSuccess is false)
			{
				return StatusCode(registerResult.Error.StatusCode, registerResult.Error);
			}
			return Ok(registerResult.Value);
		}
		[HttpPost("login-admin")]
        public async Task<IActionResult> LoginAdmin(LoginDto loginDto, CancellationToken cancellationToken = default)
		{
			var loginResult = await _userIdentityService.LoginAdmin(loginDto, cancellationToken);
			if (loginResult.isSuccess is false)
			{
				return StatusCode(loginResult.Error.StatusCode, loginResult.Error);
			}
			return Ok(loginResult.Value);
		}
		[HttpDelete("delete-admin")]
        [Authorize(policy: ApplicationStaticValue.ADMIN_POLICY_NAME)]
        public async Task<ActionResult> DeleteAdmin([FromQuery] int currentAdminId, [FromQuery] int tobeDeleteAdminId)
		{
			if (currentAdminId < 0 || tobeDeleteAdminId < 0)
				return BadRequest();
			var deleteResult = await _userIdentityService.DeleteAdmin(currentAdminId, tobeDeleteAdminId);
			if(deleteResult.isSuccess is false)
				return StatusCode(deleteResult.Error.StatusCode,deleteResult.Error);
			return Ok();
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


            var fullpath = _appsettings.ExternalUrls.FrontendBaseUrl + "/auth/external-login/";


            if (result.Succeeded)
			{
				var getUserByEmail = await _userIdentityService.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				var tokenResult = await _userIdentityService.GenerateTokensForUsers(getUserByEmail);
				if (tokenResult.isSuccess is false)
					return StatusCode(tokenResult.Error.StatusCode, tokenResult.Error);
				fullpath = fullpath + tokenResult.Value.AccessToken;
				return Redirect(fullpath);
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
				var createLogin = await _userIdentityService.UserManager.AddLoginAsync(newUser, info);

                if (createResult.isSuccess is false)
				{
					return StatusCode(createResult.Error.StatusCode, createResult.Error);
				}
				fullpath = fullpath + createResult.Value.AccessToken;
				return Redirect(fullpath);
			}
		}
		[HttpPost("change-password")]
        [Authorize(policy: ApplicationStaticValue.USER_POLICY_NAME)]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordDto changePasswordDto, [FromQuery] string userId)
		{
            /*
                        var userIdFromClaim = User.Claims.FirstOrDefault(c => c.Type.Equals(ApplicationStaticValue.UserIdClaimType))?.Value;
                        if (userIdFromClaim == null)
                            return Unauthorized();*/
            var getUser = await _userIdentityService.UserManager.FindByIdAsync(userId);
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
		[HttpGet("get-useridentity")]
		public async Task<IActionResult> GetUserIdentity(int id, CancellationToken cancellationToken = default)
		{
			var result = await _userIdentityService.GetUserIdentity(id, true);
			if(result.isSuccess is false)
			{
				return StatusCode(result.Error.StatusCode,result.Error);
			}
			return Ok(result.Value);
		}
		[HttpGet("get-useridentity-claim-from-token")]
		public async Task<IActionResult> GetUserIdentity(string accessToken, CancellationToken cancellationToken = default)
		{
			var result = await _userIdentityService.GetUserIdentity(accessToken, true);
			if (result.isSuccess is false)
			{
				return StatusCode(result.Error.StatusCode, result.Error);
			}
			return Ok(result.Value);
		}
		[HttpGet("get-user-in-role")]
		public async Task<IActionResult> GetUserIdentityInRole(int roleId, CancellationToken cancellationToken = default)
		{
			var result = await _userIdentityService.GetUsersInRole(roleId);
			if (result.isSuccess is false)
			{
				return StatusCode(result.Error.StatusCode, result.Error);
			}
			return Ok(result.Value);
		}
		[HttpGet("get-users-paging")]
		public async Task<ActionResult> GetUsers([FromQuery] int start, [FromQuery]int amount , CancellationToken cancellationToken = default)
		{
			if (start < 0 || amount < 0 || amount > PAGING_TAKE_LIMIT)
			{
				return BadRequest();
			}
			var trueStart = start * amount;
			var result = await _userIdentityService.GetUsersPaging(trueStart, amount);
			if(result.isSuccess is false)
			{
				return StatusCode(result.Error.StatusCode, result.Error);
			}
			return Ok(result.Value);
		}
		[HttpGet("reset-password")]
		public async Task<ActionResult> ResetPassword([FromQuery]ResetPasswordDto resetPasswordDto)
		{
			var confirmResetResult = await _userIdentityService.ResetPassword(resetPasswordDto);
			if (confirmResetResult.isSuccess is false)
				return StatusCode(confirmResetResult.Error.StatusCode, confirmResetResult.Error);
			return Ok("reseted");
		}
		[HttpGet("forget-password")]
		public async Task<ActionResult> ForgetPassword([FromQuery]string email)
		{
			var trySendMailResetResult = await _userIdentityService.GenerateForgotPasswordToken(email);
			if(trySendMailResetResult .isSuccess is false)
			{
				return StatusCode(trySendMailResetResult.Error.StatusCode, trySendMailResetResult.Error);
			}
			return Ok("sended");
		}
	}
}
