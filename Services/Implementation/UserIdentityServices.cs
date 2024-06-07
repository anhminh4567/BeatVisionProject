using AutoMapper;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public partial class UserIdentityServices : IUserIdentityService
	{
		public UserManager<CustomIdentityUser> UserManager { get; set; }
		public RoleManager<CustomIdentityRole> RoleManager { get; set; }
		public SignInManager<CustomIdentityUser> SigninManager { get; set; }
		private readonly ISecurityTokenServices _securityTokenServices;
		private readonly IUnitOfWork _unitOfWork;
		private readonly AppsettingBinding _settings;
		private readonly JwtSection _jwtSettings;
		private readonly IMyEmailService _mailServices;
		private readonly IMapper _mapper;
		private const string ADMIN_ROLE_NAME = "admin";
		private const string USER_ROLE_NAME = "User";
		private const int RANDOM_PASSWORD_SIZE = 10;
		public UserIdentityServices(UserManager<CustomIdentityUser> userManager, RoleManager<CustomIdentityRole> roleManager, SignInManager<CustomIdentityUser> signinManager, ISecurityTokenServices securityTokenServices, IUnitOfWork unitOfWork, AppsettingBinding settings,  IMyEmailService mailServices, IMapper mapper)
		{
			UserManager = userManager;
			RoleManager = roleManager;
			SigninManager = signinManager;
			_securityTokenServices = securityTokenServices;
			_unitOfWork = unitOfWork;
			_settings = settings;
			_mailServices = mailServices;
			_mapper = mapper;

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
				error.StatusCode = (int)HttpStatusCode.InternalServerError;
				error.isException = true;
				error.ErrorMessage = ex.Message;
				return Result<TokenResponseDto>.Fail(error);
			}
		}
		public async Task<Result<TokenResponseDto>> CreateUser(CustomIdentityUser identityUser, UserProfile userProfile)
		{
			var error = new Error();
			var tryGetUser = await UserManager.FindByEmailAsync(identityUser.Email);
			if (tryGetUser is not null )
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
			var getUsers = await UserManager.GetUsersInRoleAsync(USER_ROLE_NAME);
			var tryGetUser = await UserManager.FindByEmailAsync(loginDto.Email);
			if (tryGetUser == null || getUsers.Contains(tryGetUser) is false)
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
		public async Task<Result<TokenResponseDto>> CreateAdmin(RegisterDto registerDto)
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
				EmailConfirmed = true,
				TwoFactorEnabled = false,
				LockoutEnabled = false,
			};
			await _unitOfWork.BeginTransactionAsync();
			var createResult =await UserManager.CreateAsync(newUser,registerDto.Password);
			await _unitOfWork.SaveChangesAsync();
			if(createResult.Succeeded is false)
			{
				await _unitOfWork.RollBackAsync();
				error.ErrorMessage = "fail to create admin, try again later";
				return Result<TokenResponseDto>.Fail(error);
			}
			var addResult = await UserManager.AddToRoleAsync(newUser, ADMIN_ROLE_NAME);
			await _unitOfWork.SaveChangesAsync();
			if (addResult.Succeeded is false)
			{
				await _unitOfWork.RollBackAsync();
				error.ErrorMessage = "fail to add admin to the role, try again later";
				return Result<TokenResponseDto>.Fail(error);
			}
			//var newUserProfile = new UserProfile()
			//{
			//	IdentityId = newUser.Id,
			//	Fullname = registerDto.Fullname,
			//	AccountStatus = AcccountStatus.ACTIVE,
			//};
			//newUserProfile.IdentityId = getUser.Id;
			//newUserProfile.Fullname = registerDto.Fullname;
			//newUserProfile.AccountStatus = AcccountStatus.ACTIVE;
			//newUserProfile.TotalTrack = 0;
			//newUserProfile.TotalTrack = 0;
			//var createResult = await _unitOfWork.Repositories.userProfileRepository.Create(newUserProfile);
			var generateTokenResult = await GenerateTokensForUsers(newUser);
			if (generateTokenResult.isSuccess is false)
			{
				await _unitOfWork.RollBackAsync();
				error.StatusCode = (int)HttpStatusCode.InternalServerError;
				error.ErrorMessage = "cannot creat token ";
				return Result<TokenResponseDto>.Fail(error);
			}
			await _unitOfWork.CommitAsync();
			return Result<TokenResponseDto>.Success(generateTokenResult.Value);
		}
		public async Task<Result<TokenResponseDto>> LoginAdmin(LoginDto loginDto, CancellationToken cancellationToken)
		{
			var error = new Error();
			var getUsersAdmin = await UserManager.GetUsersInRoleAsync(ADMIN_ROLE_NAME);
			if(getUsersAdmin is null)
			{
				error.ErrorMessage = "there are no admin yet, create one";
				return Result<TokenResponseDto>.Fail(error);
			}
			var tryGetUser = getUsersAdmin.FirstOrDefault (u => u.Email ==  loginDto.Email);
			if (tryGetUser == null)
			{
				error.ErrorMessage = "user not found";
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
		public async Task<Result> DeleteAdmin(int currentAdminId, int tobeDeletedId)
		{
			var error = new Error();
			if(currentAdminId == tobeDeletedId)
			{
				error.ErrorMessage = "cannot delete yourself";
				return Result.Fail(error);
			}
			var getAdmins = await UserManager.GetUsersInRoleAsync(ADMIN_ROLE_NAME);
			if(getAdmins is null)
			{
				error.ErrorMessage = "no admin found";
				return Result.Fail(error);
			}
			var getAdminsId = getAdmins.Select(u => u.Id);
			if(getAdminsId.Contains(currentAdminId) is false || getAdminsId.Contains(tobeDeletedId) is false)
			{
				error.ErrorMessage = "admin id not found";
				return Result.Fail(error) ;	
			}
			var getAdminTobeDeleted = getAdmins.FirstOrDefault(admin => admin.Id == tobeDeletedId);
			var deleteResult = await UserManager.DeleteAsync(getAdminTobeDeleted);
			if(deleteResult.Succeeded is false)
			{
				error.ErrorMessage = "fail to deletee";
				return Result.Fail(error);
			}
			return Result.Success();
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
			claims.Add(new Claim(ApplicationStaticValue.UsernameClaimType, user.UserName));
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
namespace Services.Implementation
{
	public partial class UserIdentityServices
	{
		public async Task<Result<CustomIdentityUserDto>> GetUserIdentity(int userId, bool isIncludeDetail = false)
		{
			var error = new Error();
			CustomIdentityUser getUser;
			if (isIncludeDetail)
			{
				getUser = await _unitOfWork.Repositories.customIdentityUser.GetByIdInclude(userId, "UserClaims,Roles,UserLogins,UserTokens,UserProfile");
			}
			else
			{
				getUser = await _unitOfWork.Repositories.customIdentityUser.GetById(userId);
			}
			if (getUser is null)
			{
				error.ErrorMessage = "cannot find user identity with this id, might not have login";
				return Result<CustomIdentityUserDto>.Fail(error);
			}
			var mappedValue = _mapper.Map<CustomIdentityUserDto>(getUser);
			return Result<CustomIdentityUserDto>.Success(mappedValue);
		}
		public async Task<Result<IList<Claim>>> GetUserIdentity(string accessToken, bool isIncludeDetail = false)
		{
			var error = new Error();
			var getIdentityFromToken = _securityTokenServices.GetPrincipalFromExpiredToken(accessToken);
			var getClaims = getIdentityFromToken.Claims.ToList();
			return Result<IList<Claim>>.Success(getClaims);
		}
		public async Task<Result<IList<CustomIdentityUserDto>>> GetUsersInRole(int roleId)
		{
			var error = new Error();
			if (roleId == null || roleId <= 0)
			{
				error.ErrorMessage = "role id is not correct ";
				return Result<IList<CustomIdentityUserDto>>.Fail(error);
			}
			var getAllUserInRole = (await _unitOfWork.Repositories.customIdentityRole.GetByIdInclude(roleId, "Users"))?.Users;
			if (getAllUserInRole is null)
			{
				error.ErrorMessage = "fail to get user by id, somethin wrong ";
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<IList<CustomIdentityUserDto>>.Fail(error);
			}
			var mappedResult = _mapper.Map<IList<CustomIdentityUserDto>>(getAllUserInRole);
			return Result<IList<CustomIdentityUserDto>>.Success(mappedResult);
		}
		public async Task<Result<PagingResponseDto<  IList<CustomIdentityUserDto>>>> GetUsersPaging(int start, int amount)
		{
			var error = new Error();
			var getUsers = (await _unitOfWork.Repositories.customIdentityUser.GetByCondition(null, null, "", start, amount)).ToList();
			if (getUsers is null)
			{
				error.ErrorMessage = "fail to get user by id, somethin wrong ";
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<PagingResponseDto<IList<CustomIdentityUserDto>>>.Fail(error);
			}
			var mappedResult = _mapper.Map<IList<CustomIdentityUserDto>>(getUsers);
			return Result<PagingResponseDto<IList<CustomIdentityUserDto>>>.Success(new PagingResponseDto<IList<CustomIdentityUserDto>>
			{
				TotalCount = _unitOfWork.Repositories.customIdentityUser.COUNT,
				Value = mappedResult
			}); ;
		}
		// cach nay lam ez, nhung phien ben FE, can lam them 1 trang nua
		public async Task<Result<string>> GenerateForgotPasswordToken(string email)
		{
			var error = new Error();
			var getUserIdentity = await UserManager.FindByEmailAsync(email);
			if (getUserIdentity is null)
			{
				error.ErrorMessage = "cannot find user";
				return Result<string>.Fail(error);
			}
            var getPasswordResetTokenProvider = UserManager.Options.Tokens.PasswordResetTokenProvider;

            var generateResetPasswordToken = await UserManager.GeneratePasswordResetTokenAsync(getUserIdentity);
			if(string.IsNullOrEmpty( generateResetPasswordToken ))
			{
				error.ErrorMessage = "canot generate password reset email right now";
				return Result<string>.Fail();
			}
			var getPath = _settings.MailTemplateAbsolutePath.FirstOrDefault(p => p.TemplateName == "ForgetPasswordEmail")?.TemplateAbsolutePath;
			if(string.IsNullOrEmpty(getPath)) 
			{
				error.ErrorMessage = "canot find email template";
				return Result<string>.Fail(error);
			}
			var callbackPathUrl = $"{_settings.ExternalUrls.FrontendResetPasswordUrl}";
			UriBuilder builder = new UriBuilder(callbackPathUrl);
            var queryParam = $"?Email={WebUtility.UrlEncode(getUserIdentity.Email)}&ResetToken={WebUtility.UrlEncode(generateResetPasswordToken)}";
            builder.Query = queryParam;
            callbackPathUrl = builder.ToString();
            //TEST PURPOSE, TOBE DELETED
            //TEST PURPOSE, TOBE DELETED

            //var newTestPassword = "abcd";

            //var queryParam = $"Email={WebUtility.UrlEncode(getUserIdentity.Email)}&ResetToken={WebUtility.UrlEncode(generateResetPasswordToken)}&NewPassword={WebUtility.UrlEncode(newTestPassword)}";
            //builder.Query = queryParam ;
            //callbackPathUrl = builder.ToString() ;

            //TEST PURPOSE, TOBE DELETED
            //TEST PURPOSE, TOBE DELETED
            var forgetEmailModel = new ForgetPassworEmailModel()
			{
				PasswordResetToken = generateResetPasswordToken,
				TemporalPassword = GenerateRandomString(RANDOM_PASSWORD_SIZE),
				UserIdentity = _mapper.Map<CustomIdentityUserDto>(getUserIdentity),
				CallbackUrl = callbackPathUrl,
            };
            var mailMeta = new EmailMetaData()
            {
                ToEmail = getUserIdentity.Email,
                Subject = "forget password"
            };
            var sendResult = _mailServices.SendEmailWithTemplate<ForgetPassworEmailModel>(mailMeta,getPath,forgetEmailModel);
			return Result<string>.Success(generateResetPasswordToken);
		}
		public async Task<Result> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
            var error = new Error();
            var getUserIdentity = await UserManager.FindByEmailAsync(resetPasswordDto.Email);
            if (getUserIdentity is null)
            {
                error.ErrorMessage = "cannot find user";
                return Result.Fail(error);
            }
			var resetResult = await UserManager.ResetPasswordAsync(getUserIdentity,resetPasswordDto.ResetToken,resetPasswordDto.NewPassword);
			if(resetResult.Succeeded is false)
			{
				error.ErrorMessage = "fail to reset , try again later";
				return Result.Fail(error);
			}
			return Result.Success();
		}
        public async Task<Result> IsUserIdentityLegit(CustomIdentityUser user)
		{
			var error = new Error();
			if ( (await UserManager.IsEmailConfirmedAsync(user)) is false)
			{
				error.ErrorMessage = "email not confirmed";
				return Result.Fail();
			}
			if( (await UserManager.IsLockedOutAsync(user)) is true)
			{
				error.ErrorMessage = "account is lock out";
				return Result.Fail();
			}
			return Result.Success();
		}
		private string GenerateRandomString(int size)
		{
			string randomAllowString = "1234567890qwertyuiopasdfghjklzxcvbnm";
			var randomAllowStringSize = randomAllowString.Length;
			var random = new Random();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i  = 0; i < size; i++)
			{
				var getPosition = random.Next(randomAllowStringSize);
				stringBuilder.Append(randomAllowString[getPosition]);
			}
			return stringBuilder.ToString();
		}
		
	}

}
