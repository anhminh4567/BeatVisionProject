using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.RequestDto;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
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
		[HttpGet("server-get-range")]
		public async Task<ActionResult> AdminGetAllNotiServer([FromQuery] int start, [FromQuery] int take, [FromQuery] NotificationType? type, [FromQuery] bool orderDate = false)
		{
			if (start < 0 || take < 0)
				return BadRequest();
			var trueStart = start * take;
			var getResult = await _notificationManager.GetServerNotificationRange(trueStart,take,type,orderDate);
			if (getResult.isSuccess is false)
				return StatusCode(getResult.Error.StatusCode, getResult.Error);
			return Ok(getResult.Value);
		}
		//[HttpPost("to-users")]
		//public async Task<ActionResult> AdminCreateNotificationToUser([FromForm]CreateMessageDto createMessageDto, [FromForm] IList<int> userIds)
		//{
		//	var sendResult =await  _notificationManager.ServerSendNotificationMail_ToGroups(createMessageDto, userIds.ToArray());
		//	if (sendResult.isSuccess is false)
		//		return StatusCode(sendResult.Error.StatusCode, sendResult.Error);
		//	return Ok();
		//}
		[HttpPost("admin-create-notification")]
		public async Task<ActionResult> AdminCreateNotification([FromForm] AdminCreateMessageDto adminCreateMessageDto)
		{
			if(adminCreateMessageDto.Type == NotificationType.SINGLE && adminCreateMessageDto.UserId is null )
				return BadRequest();
			if (adminCreateMessageDto.Type == NotificationType.SINGLE && adminCreateMessageDto.UserId <= 0)
				return BadRequest();
			var createResult = await _notificationManager.AdminCreateNotification(adminCreateMessageDto);
			if(createResult.isSuccess is false)
				return StatusCode(createResult.Error.StatusCode,createResult.Error);
			return Ok();
		}
		[HttpDelete("delete-message")]
		public async Task<ActionResult> AdminRemoveMessage([FromQuery]int messageId)
		{
			if (messageId <= 0)
				return BadRequest();
			var removeMessageResult = await _notificationManager.AdminRemoveMessage(messageId);
			if (removeMessageResult.isSuccess is false)
				return StatusCode(removeMessageResult.Error.StatusCode,removeMessageResult.Error);
			return Ok();
		}
		//[HttpGet("admin-to-single-subscriber")]
		//public async Task<ActionResult> AdminCreateNotificationToSingle([FromQuery] int profileId)
		//{
		//	var messageDto = new CreateMessageDto
		//	{
		//		Content = $"your have just bought  track",
		//		MessageName = "please get your track in the order section on our site",
		//		Weight = NotificationWeight.MINOR,
		//	};
		//	var result = await _notificationManager.ServerSendNotificationMail_ToUser(messageDto,profileId);
		//	return Ok();
		//}
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
