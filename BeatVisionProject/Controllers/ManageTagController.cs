﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared;
using Shared.ConfigurationBinding;
using Shared.Models;
using Shared.RequestDto;
using Shared.ResponseDto;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageTagController : ControllerBase
	{
		private readonly TagManager _tagManager;
		private readonly AppsettingBinding _appsettings;

		public ManageTagController(TagManager tagManager, AppsettingBinding appsettings)
		{
			_tagManager = tagManager;
			_appsettings = appsettings;
		}

		[HttpGet]
		public async Task<ActionResult<IList<TagDto>>> GetAll()
		{
			var isIt = User.IsInRole(ApplicationStaticValue.ADMIN_ROLE);
			return Ok(await _tagManager.GetAll());
		}
		[HttpPost]
        //[Authorize(policy: ApplicationStaticValue.ADMIN_POLICY_NAME)]
        public async Task<ActionResult<TagDto?>> CreateTag([FromForm]CreateTagDto createTagDto)
		{
			var createResult = await _tagManager.Create(createTagDto);
			if(createResult == null)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
			return Ok(createResult);
		}
		[HttpDelete("{id}")]
        //[Authorize(policy: ApplicationStaticValue.ADMIN_POLICY_NAME)]
        public async Task<ActionResult> DeleteTag([FromRoute]int id)
		{
			var deleteResult = await _tagManager.Remove(id);
			return Ok(deleteResult);
		}
	}
}
