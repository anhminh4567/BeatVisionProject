using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
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
		private readonly IUnitOfWork _unitOfWork;

		public ManageRoleClaimController(UserService manageUserService, UserIdentityServices userIdentityService, IUnitOfWork unitOfWork)
		{
			_manageUserService = manageUserService;
			_userIdentityService = userIdentityService;
			_unitOfWork = unitOfWork;
		}

		[HttpGet("get-all")]
		public async Task<ActionResult> GetAll()
		{
			return Ok(await _unitOfWork.Repositories.customIdentityUserRole.GetAll());
		}
		[HttpGet]
		public async Task<ActionResult> GetById(int roleId)
		{
			return Ok(await _userIdentityService.RoleManager.FindByIdAsync(roleId.ToString()));
		}
		[HttpPost]
		public async Task<IActionResult> AddRole([FromForm]CreateRoleDto newRoleDto)
		{
			var createRoleResult = await _userIdentityService.AddRole(newRoleDto);
			if (createRoleResult.isSuccess is false)
				return StatusCode(createRoleResult.Error.StatusCode, createRoleResult.Error);
			return Ok();
		}
		[HttpPut]
		public async Task<IActionResult> UpdateRole([FromForm]UpdateRoleDto updateDto)
		{
			var updateResult = await _userIdentityService.UpdateRole(updateDto);
			if (updateResult.isSuccess is false)
				return StatusCode(updateResult.Error.StatusCode, updateResult.Error);
			return Ok();
		} 
		[HttpDelete]
		public async Task<ActionResult> DeleteRole([FromQuery]int id)
		{
			var getRole = await _userIdentityService.RoleManager.FindByIdAsync(id.ToString());
			if (getRole == null)
				return BadRequest();
			var deleteResult = await _userIdentityService.RoleManager.DeleteAsync(getRole);
			if (deleteResult.Succeeded is false)
				return StatusCode(StatusCodes.Status500InternalServerError, "cant delete rightnow");
			return Ok();
		}
	}
}
