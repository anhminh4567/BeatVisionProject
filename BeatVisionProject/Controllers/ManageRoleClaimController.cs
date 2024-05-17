using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared.RequestDto;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageRoleClaimController : ControllerBase
	{
		private readonly UserService _manageUserService;
		private readonly UserIdentityServices _userIdentityService;

		public ManageRoleClaimController(UserService manageUserService, UserIdentityServices userIdentityService)
		{
			_manageUserService = manageUserService;
			_userIdentityService = userIdentityService;
		}

		[HttpPost("add-role")]
		public async Task<IActionResult> AddRole(CreateRoleDto newRoleDto)
		{
			var createRoleResult = await _userIdentityService.AddRole(newRoleDto);
			if (createRoleResult.isSuccess is false)
				return StatusCode(createRoleResult.Error.StatusCode, createRoleResult.Error);
			return Ok();
		}
		[HttpPost("update-role")]
		public async Task<IActionResult> UpdateRole(UpdateRoleDto updateDto)
		{
			var updateResult = await _userIdentityService.UpdateRole(updateDto);
			if (updateResult.isSuccess is false)
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			return Ok();
		}
	}
}
