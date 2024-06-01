using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.RequestDto;

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
		
	}
}
