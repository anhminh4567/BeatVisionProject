using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.RequestDto;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageNotificationController : ControllerBase
	{
		private readonly NotificationManager _notificationManager;
		private readonly AppsettingBinding _appsettings;
		private readonly AppUserManager _appUserManager;

		public ManageNotificationController(NotificationManager notificationManager, AppsettingBinding appsettings, AppUserManager appUserManager)
		{
			_notificationManager = notificationManager;
			_appsettings = appsettings;
			_appUserManager = appUserManager;
		}

		[HttpPost("to-users")]
		public async Task<ActionResult> AdminCreateNotificationToUser([FromForm]CreateMessageDto createMessageDto, [FromForm] IList<int> userIds)
		{
			var sendResult =await  _notificationManager.ServerSendNotificationMail_ToGroups(createMessageDto, userIds.ToArray());
			if (sendResult.isSuccess is false)
				return StatusCode(sendResult.Error.StatusCode, sendResult.Error);
			return Ok();
		}
		[HttpPost("admin-to-all")]
		public async Task<ActionResult> AdminCreateNotification()
		{
			return Ok();
		}
		[HttpPost("admin-to-all-subscriber")]
		public async Task<ActionResult> AdminCreateNotificationToSubscriber()
		{
			return Ok();
		}
		[HttpGet("admin-to-single-subscriber")]
		public async Task<ActionResult> AdminCreateNotificationToSingle([FromQuery] int profileId)
		{
			var messageDto = new CreateMessageDto
			{
				Content = $"your have just bought  track",
				MessageName = "please get your track in the order section on our site",
				Weight = NotificationWeight.MINOR,
			};
			var result = await _notificationManager.ServerSendNotificationMail_ToUser(messageDto,profileId);
			return Ok();
		}
		[HttpGet("get-all")]
		public async Task<ActionResult> GetUserNotification([FromQuery] int userProfileId)
		{
			if(userProfileId <= 0) 
				return BadRequest();
			var result =  await _notificationManager.GetUserNotifications(userProfileId);
			if(result.isSuccess is false)
				return StatusCode(result.Error.StatusCode,result.Error);
			return Ok(result.Value);
		}
		[HttpGet("read")]
		public async Task<ActionResult> ReadNoti([FromQuery] int userProfileId, [FromQuery] int messageId) 
		{
			if (userProfileId <= 0 || messageId <= 0)
				return BadRequest();
			var result = await _notificationManager.ReadNotification(userProfileId, messageId);	
			if(result.isSuccess is false)
				return StatusCode(result.Error.StatusCode, result.Error);
			return Ok();
		}
		[HttpGet("read-all")]
		public async Task<ActionResult> ReadNotiAll([FromQuery] int userProfileId)
		{
			if (userProfileId <= 0 )
				return BadRequest();
			var result = await _notificationManager.ReadAllNotifications(userProfileId);
			if (result.isSuccess is false)
				return StatusCode(result.Error.StatusCode, result.Error);
			return Ok();
		}
	}
}
