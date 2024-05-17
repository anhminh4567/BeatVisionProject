﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Helper;
using Shared.IdentityConfiguration;
using Shared.Models;
using Shared.RequestDto;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IUserIdentityService
    {
        SignInManager<CustomIdentityUser> SigninManager { get;set;}
        RoleManager<CustomIdentityRole> RoleManager{get;set;}
        UserManager<CustomIdentityUser> UserManager { get; set;}
        Task<Result<TokenResponseDto>> Register(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<TokenResponseDto>> Login(LoginDto loginDto, CancellationToken cancellationToken=default);
		Task<Result<TokenResponseDto>> CreateUser(CustomIdentityUser identityUser, UserProfile userProfile);

		Task<Result> Logout(CustomIdentityUser user);
        Task<Result<TokenResponseDto>> Refresh(string accessToken, string refreshToken);
        Task<Result> ChangePassword(CustomIdentityUser user, string oldPassword, string newPassword);
        Task<Result<IActionResult>> ExternalLogin(string externalProviderSchemeName, string redirectUrl);
        
        Task<Result> AddRole(CreateRoleDto newRoleDto);
        Task<Result> UpdateRole(UpdateRoleDto newRoleDto);
        Task<Result> RemoveRole(int roleId);

        Task<Result<TokenResponseDto>> GenerateTokensForUsers(CustomIdentityUser user);
        Task<Result> SendConfirmEmail(CustomIdentityUser user, string callbackUrl, CancellationToken cancellationToken=default);
        Task<Result> ConfirmEmailToken(string userId, string emailToken);

	}
}
