using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Services.Interface;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.Helper;
using Shared.IdentityConfiguration;
using Shared.Models;
using Shared.Poco;
using Shared.RequestDto;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class UserIdentityServices : IUserIdentityService
	{
		public UserManager<CustomIdentityUser> UserManager { get; set; }
		public RoleManager<CustomIdentityRole> RoleManager { get; set; }
		public SignInManager<CustomIdentityUser> SigninManager { get; set; }
		private readonly ISecurityTokenServices _securityTokenServices;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _settings;
		private readonly JwtSection _jwtSettings;
		private readonly IMyEmailService _mailServices;

		public UserIdentityServices(UserManager<CustomIdentityUser> userManager, RoleManager<CustomIdentityRole> roleManager, SignInManager<CustomIdentityUser> signinManager, ISecurityTokenServices securityTokenServices, IUnitOfWork unitOfWork, AppsettingBinding settings, IMyEmailService mailServices)
		{
			UserManager = userManager;
			RoleManager = roleManager;
			SigninManager = signinManager;
			_securityTokenServices = securityTokenServices;
			_unitOfWork = unitOfWork;
			_settings = settings;
			_mailServices = mailServices;
			_jwtSettings = _settings.JwtSection;
		}

		public async Task<Result<TokenResponseDto>> Register(RegisterDto registerDto, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			var tryGetUser = await UserManager.FindByEmailAsync(registerDto.Email);
			if (tryGetUser is not null)
			{
				error.ErrorMessage = "user already exist";
				error.StatusCode = StatusCodes.Status400BadRequest;
				return Result<TokenResponseDto>.Fail(error);
			}
			var newUser = new CustomIdentityUser()
			{
				Email = registerDto.Email,
				UserName = registerDto.Email,
				//Dob = registerDto.Dob,
				SecurityStamp = Guid.NewGuid().ToString(),
				ConcurrencyStamp = Guid.NewGuid().ToString(),
				EmailConfirmed = false,
				TwoFactorEnabled = false,
				LockoutEnabled = true,
			};
			var newUserProfile = new UserProfile();
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var registerResult = await UserManager.CreateAsync(newUser, registerDto.Password);
				await _unitOfWork.SaveChangesAsync();
				if (registerResult.Succeeded)
				{
					var getUser = await UserManager.FindByEmailAsync(newUser.Email);
					newUserProfile.IdentityId = getUser.Id;
					newUserProfile.Fullname = registerDto.Fullname;
					newUserProfile.AccountStatus = AcccountStatus.ACTIVE;
					newUserProfile.TotalTrack = 0;
					newUserProfile.TotalTrack = 0;
					var createResult = await _unitOfWork.Repositories.userProfileRepository.Create(newUserProfile);
					await _unitOfWork.SaveChangesAsync();
					await UserManager.AddToRoleAsync(getUser, RolesEnum.User.ToString());
					await _unitOfWork.SaveChangesAsync();
					//throw new Exception();
					var generateTokenResult = await GenerateTokensForUsers(getUser);
					if (generateTokenResult.isSuccess is false)
					{
						await _unitOfWork.RollBackAsync();
						error.StatusCode = (int)HttpStatusCode.InternalServerError;
						error.ErrorMessage = "cannot creat token ";
						return Result<TokenResponseDto>.Fail(error);
					}
					else
					{
						await _unitOfWork.CommitAsync();
						return Result<TokenResponseDto>.Success(generateTokenResult.Value);
					}
				}
				else
				{
					await _unitOfWork.RollBackAsync();
					error.ErrorMessage = "fail to create user";
					error.StatusCode = (int)HttpStatusCode.InternalServerError;
					return Result<TokenResponseDto>.Fail(error);
				}
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.StatusCode= (int)HttpStatusCode.InternalServerError;
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result<TokenResponseDto>.Fail(error);
			}
		}
		public async Task<Result<TokenResponseDto>> CreateUser(CustomIdentityUser identityUser, UserProfile userProfile)
		{
			var error = new Error();
			var tryGetUser = await UserManager.FindByEmailAsync(identityUser.Email);
			if (tryGetUser is not null)
			{
				error.ErrorMessage = "user already exist";
				error.StatusCode = StatusCodes.Status400BadRequest;
				return Result<TokenResponseDto>.Fail(error);
			}
			try
			{
				await _unitOfWork.BeginTransactionAsync();
				var createResult = await UserManager.CreateAsync(identityUser);
				await _unitOfWork.SaveChangesAsync();
				if (createResult.Succeeded)
				{
					var getUser = await UserManager.FindByEmailAsync(identityUser.Email);
					userProfile.IdentityId = identityUser.Id;
					var createUserProfileResult = await _unitOfWork.Repositories.userProfileRepository.Create(userProfile);
					await _unitOfWork.SaveChangesAsync();
					await UserManager.AddToRoleAsync(getUser, RolesEnum.User.ToString());
					await _unitOfWork.SaveChangesAsync();
					var generateTokenResult = await GenerateTokensForUsers(getUser);
					if (generateTokenResult.isSuccess is false)
					{
						await _unitOfWork.RollBackAsync();
						error.StatusCode = (int)HttpStatusCode.InternalServerError;
						error.ErrorMessage = "cannot creat token ";
						return Result<TokenResponseDto>.Fail(error);
					}
					else
					{
						await _unitOfWork.CommitAsync();
						return Result<TokenResponseDto>.Success(generateTokenResult.Value);
					}
				}
				else
				{
					await _unitOfWork.RollBackAsync();
					error.ErrorMessage = "fail to create user";
					error.StatusCode = (int)HttpStatusCode.InternalServerError;
					return Result<TokenResponseDto>.Fail(error);
				}
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollBackAsync();
				error.StatusCode = (int)HttpStatusCode.InternalServerError;
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result<TokenResponseDto>.Fail(error);
			}
		}
		public async Task<Result<TokenResponseDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
		{
			var error = new Error();
			var tryGetUser = await UserManager.FindByEmailAsync(loginDto.Email);
			if (tryGetUser == null)
			{
				error.ErrorMessage = "user not found";
				error.StatusCode = (int)HttpStatusCode.BadRequest;
				return Result<TokenResponseDto>.Fail(error);
			}
			var loginResult = await UserManager.CheckPasswordAsync(tryGetUser, loginDto.Password);
			if (loginResult)
			{
				var generateTokenResult = await GenerateTokensForUsers(tryGetUser);
				if (generateTokenResult.isSuccess is false)
				{
					error.StatusCode = (int)HttpStatusCode.InternalServerError;
					error.ErrorMessage = "cannot creat token ";
					return Result<TokenResponseDto>.Fail(error);
				}
				else
				{
					return Result<TokenResponseDto>.Success(generateTokenResult.Value);
				}
			}
			error.ErrorMessage = "user fail to login";
			error.StatusCode = (int)HttpStatusCode.BadRequest;
			return Result<TokenResponseDto>.Fail(error);
		}
		public async Task<Result> Logout(CustomIdentityUser user)
		{
			var removeResult = await UserManager.RemoveAuthenticationTokenAsync(user, ApplicationStaticValue.ApplicationTokenIssuer, "RefreshToken");
			if (removeResult.Succeeded)
			{
				return Result.Success();
			}
			else
			{
				return Result.Fail();
			}
		}
		public async Task<Result<TokenResponseDto>> Refresh(string accessToken, string refreshToken)
		{
			var error = new Error();
			var getClaimsPrincipleFromToken = _securityTokenServices.GetPrincipalFromExpiredToken(accessToken);
			if (getClaimsPrincipleFromToken is null)
			{
				error.ErrorMessage = "the token is not valid to begin with, login again";
				return Result<TokenResponseDto>.Fail(error);
			}
			var userId = getClaimsPrincipleFromToken.Claims.First(claim => claim.Type.Equals(ApplicationStaticValue.UserIdClaimType)).Value;
			var getUser = await UserManager.FindByIdAsync(userId);
			var getUserRefreshToken = (await _unitOfWork
				.Repositories
				.customIdentityUserToken
				.GetByCondition(t => t.UserId == getUser.Id &&
								t.LoginProvider == ApplicationStaticValue.ApplicationTokenIssuer &&
								t.Name == "RefreshToken")
				).FirstOrDefault();
			//var checkResult = await UserManager.VerifyUserTokenAsync(getUser, ApplicationStaticValue.ApplicationTokenIssuer, "RefreshToken",refreshToken);
			if (getUserRefreshToken is null)
			{
				error.ErrorMessage = "refresh token is not correct, login again";
				return Result<TokenResponseDto>.Fail(error);
			}
			var validateTokenResult = ((refreshToken == getUserRefreshToken.Value) && (DateTime.Now < getUserRefreshToken.ExpiredDate));
			if (validateTokenResult is false)
			{
				error.ErrorMessage = "refresh token is expired, try login again";
				return Result<TokenResponseDto>.Fail(error);
			}
			return await GenerateTokensForUsers(getUser);
		}
		public async Task<Result<IActionResult>> ExternalLogin(string externalProviderSchemeName, string redirectUrl)
		{
			var authProperties = new AuthenticationProperties();
			var getAuthProviderSchemes = await SigninManager.GetExternalAuthenticationSchemesAsync();
			bool isSchemeExist = false;
			foreach (var authScheme in getAuthProviderSchemes)
			{
				if (string.Equals(authScheme.Name, externalProviderSchemeName))
				{
					isSchemeExist = true;
					break;
				}
			}
			if (isSchemeExist is false)
			{
				return Result<IActionResult>.Fail(new Error()
				{
					StatusCode = (int)HttpStatusCode.BadRequest,
					ErrorMessage = "the provided scheme is not exist or not support by the app"
				});
			}
			authProperties = SigninManager.ConfigureExternalAuthenticationProperties(externalProviderSchemeName, redirectUrl);
			return Result<IActionResult>.Success(new ChallengeResult(externalProviderSchemeName, authProperties));
		}
		public async Task<Result> ChangePassword(CustomIdentityUser user, string oldPassword, string newPassword)
		{
			var error = new Error();
			if (string.IsNullOrEmpty(user.PasswordHash))
			{
				error.ErrorMessage = "user does not have password, you might registered as external oauth so we dont manage your password";
				return Result.Fail();
			}
			var isOldPassMatch = await UserManager.CheckPasswordAsync(user, oldPassword);
			if (isOldPassMatch is false)
			{
				return Result.Fail();
			}
			var changeResult = await UserManager.ChangePasswordAsync(user, oldPassword, newPassword);
			if (changeResult.Succeeded)
			{
				return Result.Success();
			}
			error.StatusCode = StatusCodes.Status500InternalServerError;
			error.ErrorMessage = "cannot change password right now";
			return Result.Fail(error);
		}
		public async Task<Result> AddRole(CreateRoleDto newRoleDto)
		{
			var isRoleExist = await RoleManager.RoleExistsAsync(newRoleDto.RoleName);
			if (isRoleExist is true)
			{
				return Result.Fail();
			}
			var newRole = new CustomIdentityRole()
			{
				ConcurrencyStamp = Guid.NewGuid().ToString(),
				Description = newRoleDto.Description,
				Name = newRoleDto.RoleName,
			};
			var createResult = await RoleManager.CreateAsync(newRole);
			if (createResult.Succeeded is false)
			{
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result> UpdateRole(UpdateRoleDto updateRoleDto)
		{
			var getRole = await RoleManager.FindByIdAsync(updateRoleDto.RoleId.ToString());
			if (getRole is null)
			{
				return Result.Fail();
			}
			getRole.Name = updateRoleDto.RoleName;
			getRole.Description = updateRoleDto.Description;
			var updateResult = await RoleManager.UpdateAsync(getRole);
			if (updateResult.Succeeded is false)
			{
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result> RemoveRole(int roleId)
		{
			var getRole = await RoleManager.FindByIdAsync(roleId.ToString());
			if (getRole is null)
			{
				return Result.Fail();
			}
			var deleteResult = await RoleManager.DeleteAsync(getRole);
			if (deleteResult.Succeeded is false) 
			{ 
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result<TokenResponseDto>> GenerateTokensForUsers(CustomIdentityUser user)
		{
			var getUserRoles = await UserManager.GetRolesAsync(user);
			var claims = CreateClaimsForUser(user, getUserRoles.ToArray());
			var createAccess = _securityTokenServices.GenerateAccessToken(claims);
			var creatRefresh = _securityTokenServices.GenerateRefreshToken();
			await UserManager.SetAuthenticationTokenAsync(user, ApplicationStaticValue.ApplicationTokenIssuer, "RefreshToken", creatRefresh.refreshToken);
			var getToken = (await _unitOfWork.Repositories.customIdentityUserToken
				.GetByCondition(u =>
				u.UserId == user.Id &&
				u.Name == "RefreshToken" &&
				u.LoginProvider == ApplicationStaticValue.ApplicationTokenIssuer))
				.First();
			getToken.ExpiredDate = DateTime.Now.AddHours(_jwtSettings.ExpiredRefreshToken_Hour);
			await _unitOfWork.Repositories.customIdentityUserToken.Update(getToken);
			await _unitOfWork.SaveChangesAsync();
			return Result<TokenResponseDto>.Success(new TokenResponseDto
			{
				AccessToken = createAccess.accessToken,
				AccessToken_Expired = createAccess.expiredDate,
				RefreshToken = creatRefresh.refreshToken,
				RefreshToken_Expired = creatRefresh.expiredDate,
			});
		}
		private List<Claim> CreateClaimsForUser(CustomIdentityUser user, string[] RoleNames)
		{
			var claims = new List<Claim>();
			claims.Add(new Claim(ApplicationStaticValue.MailClaimType, user.Email));
			claims.Add(new Claim(ApplicationStaticValue.UserIdClaimType, user.Id.ToString()));
			foreach (var role in RoleNames)
			{
				claims.Add(new Claim(ApplicationStaticValue.UserRoleClaimType, role));
			}
			return claims;
		}

		public async Task<Result> SendConfirmEmail(CustomIdentityUser user, string callbackUrl, CancellationToken cancellationToken = default)
		{
			var error = new Error();
			if (user.EmailConfirmed)
			{
				error.ErrorMessage = "the user mail is confirmed";
				return Result.Fail(error);
			}
			var secretCode = await UserManager.GenerateEmailConfirmationTokenAsync(user);
			var metadata = new EmailMetaData()
			{
				ToEmail = user.Email,
				Subject = "confirmation email",
			};
			var emailModel = new ConfirmEmailModel()
			{
				CallbackUrl = callbackUrl,
				ConfirmationToken = secretCode,
				UserId = user.Id,
				ReceiverEmail = user.Email,
			};
			var result = await _mailServices.SendConfirmationEmail(user, metadata, emailModel, cancellationToken);
			if (result.isSuccess is false)
			{
				error.StatusCode = StatusCodes.Status500InternalServerError;
				error.ErrorMessage = "fail to send email";
				return Result.Fail();
			}
			return Result.Success();
		}
		public async Task<Result> ConfirmEmailToken(string userId, string emailToken)
		{
			var getUser = await UserManager.FindByIdAsync(userId);
			if (getUser is null)
				return Result.Fail();
			return await ConfirmEmailToken(getUser, emailToken);
		}
		private async Task<Result> ConfirmEmailToken(CustomIdentityUser user, string emailToken)
		{
			var checkResult = await UserManager.ConfirmEmailAsync(user, emailToken);
			if (checkResult.Succeeded is false)
				return Result.Fail();

			return Result.Success();
		}


	}
}
